using Models.LearningNetcoreWebApp;
using Nest;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LearningNetcoreWebApp
{
    public class GlobalSearch : ISearch
    {
        ElasticClient client = null;
        public GlobalSearch()
        {
            var uri = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(uri);
            client = new ElasticClient(settings);
            settings.DefaultIndex("car");
        }

        public IEnumerable<SuggestOption<CarList>> GetResult(string condition)
        {

            if (client.IndexExists("vehiclecars").Exists)
            {
                //var query = condition;
                //var res = client.Search<make>(s => s
                //.Query(q => q
                //.Match(m => m
                //.Field(f => f.Name)
                //.Field(f => f.Year)
                //.Query(query)))
                //);
                //var doc = res.Documents;
                //return doc.ToList<make>();

                var query = condition;
                var res = client.Search<CarList>(s => s
                .Index("vehiclecars")
    //            .Aggregations(a => a
    //        .Terms("year", st => st
    //        .Field(o => o.payload.Year)
    //        .Size(10)
    //        .Aggregations(aa => aa
    //            .Max("max_Id", m => m
    //                .Field(o => o.Id)
    //            )
    //        )
    //    )
    //)
                .Suggest(su => su
                .Completion("mm_suggest", cs => cs                
                .Prefix(query)
                .Field(f => f.mm_suggest)
                .Fuzzy(f => f
                    .Fuzziness(Fuzziness.Auto)
                )
                .Size(10)
            )
        )
    );
                var res1 = client.Search<CarList>(s => s
                .Index("vehiclecars")                
                .Aggregations(a => a                
            .Terms("SoldCount", st => st
            .Field(f => f.payload.SoldCount)
            .Size(10)
            
        )
            ));
                var doc = res.Suggest["mm_suggest"][0].Options;
                var doc1 = res1.Documents;

                return doc.Cast<SuggestOption<CarList>>();
            }
            return null;
        }

        public bool AddNewIndex(make model)
        {
            try
            {
                client.IndexAsync<make>(model, null);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception occured in AddNewIndex" + ex); ;
                return false;
            }
        }

        public IEnumerable<SuggestOption<CarList>> GetResult(int year, string term)
        {
            return GetResult(term);
        }

        public string GetTerm(string term)
        {
            if (!String.IsNullOrWhiteSpace(term))
                return Regex.Replace(term.Trim().Replace("-", " "), "[^/\\-0-9a-zA-Z\\s]*", "");
            return string.Empty;
        }

        public List<Suggest> GetCarSuggestion(NameValueCollection nvc)
        {
            List<Suggest> cityResults = null;
            try
            {
                var suggestions = GetSuggestion<make>(GetTerm(nvc["term"]), "mm_suggest", 10, "vehiclecars", null);

                if (suggestions != null)
                {
                    cityResults = new List<Suggest>();
                    foreach (var item in suggestions)
                    {
                        cityResults.Add(new Suggest() { Result = item.Source.Name });
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ("search term : ");
                Console.WriteLine(msg);
            }

            return cityResults;
        }

        private void CreateIndex(List<CarList> carList)
        {
            string indexName = "vehiclecars";
            if (!client.IndexExists(indexName).Exists)
            {
                var response = client.CreateIndex(indexName,
                ind => ind
             .Settings(s => s.NumberOfShards(1)
                 .NumberOfReplicas(2)
             )
            .Mappings(m => m
                          .Map<CarList>(type => type.AutoMap()
                              .Properties(prop => prop
                                  .Completion(c => c
                                      .Name(pN => pN.mm_suggest)
                                      .Analyzer("standard")
                                      .SearchAnalyzer("standard")
                                      .PreserveSeparators(false))))));

                //
                // return client.Index<T>(item, ind => ind.Index(indexName).Id(id));
            }
            AddDocumentES<CarList>(carList, indexName, obj => obj.Id);
        }        

        private IBulkResponse AddDocumentES<T>(List<T> itemList, string indexName, Func<T, string> fieldSelector) where T : class
        {
            IBulkResponse bulkResponse = null;
            try
            {
                int bulkSizePerRequest = 10;
                int sz = itemList.Count;
                int j;
                for (int i = 0; i < sz;)
                {
                    var descriptor = new BulkDescriptor();
                    for (j = i; j < i + bulkSizePerRequest && j < sz; j++)
                    {
                        descriptor.Index<T>(ind => ind.Index(indexName)
                            .Document(itemList.ElementAt(j)).Id(fieldSelector(itemList.ElementAt(j))));
                    }
                    i = j;
                    bulkResponse = client.Bulk(descriptor);
                }
            }
            catch (Exception ex)
            {
                string msg = ("search term : " + ex);
                Console.WriteLine(msg);
                //log.Error(MethodBase.GetCurrentMethod().Name, ex);
            }
            return bulkResponse;
        }
        

        public bool CreateIndex()
        {
            try
            {
                var data = AddDocument();
                CreateIndex(data);
                return true;
            }
            catch (Exception ex)
            {
                string msg = ("search term : " + ex);
                Console.WriteLine(msg);
                return false;
            }
        }
        private List<CarList> AddDocument()
        {
            var list = PopulateCars.GetAllCars();
            List<CarList> cars = new List<CarList>();
            foreach (var item in list)
            {
                var temp = new CarList();
                temp.Id = item.Id.ToString();
                temp.name = item.Name.Trim();                
                temp.mm_suggest = new CarSuggestion();
                temp.output = item.Name;
                string vehicleName = item.Name.Trim();
                temp.mm_suggest.weight = item.Wt;

                temp.payload = new Payload()
                {
                    Id = item.Id,
                    Name = item.Name.Trim(),
                    Year = item.Year,
                    SoldCount = item.SoldCount,
                    DisplayName = item.Name.Trim()
                };

                temp.mm_suggest.input = new List<string>();
                vehicleName = vehicleName.Replace('-', ' ');                                      //Remove - From Name
                string[] combinations = vehicleName.Split(' ');                                //Break City in Diff Token
                combinations = combinations.Where(IsDigitsOnlyInverse).ToArray();
                temp.mm_suggest.input.Add(temp.output);                    //Add output as input
                                                                           //Generate all combination of a string
                int l = combinations.Length;
                for (int p = 1; p <= l; p++)
                {
                    printSeq(l, p, combinations, temp);                                  //Add All Tokens
                }

                cars.Add(temp);
            }
            return cars;
        }

        private static bool IsDigitsOnlyInverse(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return true;
            }
            return false;
        }
        private static void printSeqUtil(int n, int k, ref int len, int[] arr, string[] combination, CarList obj)
        {
            if (len == k)                                                                       //If length of current increasing sequence becomes k, print it
            {
                if (k == 1)
                    obj.mm_suggest.input.Add(String.Format("{0}", combination[arr[0] - 1].Trim()));
                else if (k == 2)
                    obj.mm_suggest.input.Add(String.Format("{0} {1}", combination[arr[0] - 1].Trim(), combination[arr[1] - 1].Trim()));
                else if (k == 3)
                    obj.mm_suggest.input.Add(String.Format("{0} {1} {2}", combination[arr[0] - 1].Trim(), combination[arr[1] - 1].Trim(), combination[arr[2] - 1].Trim()));
                else if (k == 4)
                    obj.mm_suggest.input.Add(String.Format("{0} {1} {2} {3}", combination[arr[0] - 1].Trim(), combination[arr[1] - 1].Trim(), combination[arr[2] - 1].Trim(), combination[arr[3] - 1].Trim()));
                return;
            }

            int i = (len == 0) ? 1 : arr[len - 1] + 1;
            len++;	                                                                            // Increase length
            // Put all numbers (which are greater than the previous element) at new position.
            while (i <= n)
            {
                arr[len - 1] = i;
                printSeqUtil(n, k, ref len, arr, combination, obj);
                i++;
            }
            // This is important. The variable 'len' is shared among all function calls in recursion tree. Its value must be brought back before next iteration of while loop
            len--;
        }

        private static void printSeq(int l, int p, string[] combination, CarList obj)
        {
            int[] arr = new int[p];
            int len = 0;
            printSeqUtil(l, p, ref len, arr, combination, obj);
        }

        private IEnumerable<Nest.SuggestOption<T>> GetSuggestion<T>(string searchTerm, string completion_field, int size, string index, List<string> context) where T : class
        {
            IEnumerable<Nest.SuggestOption<T>> _suggestionList = null;
            try
            {
                Func<SearchDescriptor<T>, SearchDescriptor<T>> selectorWithContext = null;
                Func<SuggestContextQueriesDescriptor<T>, IPromise<System.Collections.Generic.IDictionary<string, System.Collections.Generic.IList<ISuggestContextQuery>>>> contentDict = null;
                if (context != null)
                {
                    contentDict =
                        new Func<SuggestContextQueriesDescriptor<T>, IPromise<IDictionary<string, IList<ISuggestContextQuery>>>>(cc => cc
                            .Context("types", context.Select<string, Func<SuggestContextQueryDescriptor<T>, ISuggestContextQuery>>
                                    (v => cd => cd.Context(v)).ToArray()));
                    selectorWithContext = new Func<SearchDescriptor<T>, SearchDescriptor<T>>(sd => sd.Index(index)
               .Suggest(s => s.Completion(completion_field, c => c.Field(completion_field).Prefix(searchTerm).Contexts(contentDict).Size(size))));
                }
                Func<SearchDescriptor<T>, SearchDescriptor<T>> selectorWithoutContext = new Func<SearchDescriptor<T>, SearchDescriptor<T>>(sd => sd
                    .Index(index).Suggest(s => s
                        .Completion(completion_field, c => c
                            .Prefix(searchTerm)
                            .Field(completion_field).Size(size)))
                            );


                ISearchResponse<T> _result = client.Search<T>(context == null ? selectorWithoutContext : selectorWithContext);

                if (_result.Suggest[completion_field][0].Options.Count <= 0)
                {
                    Func<SearchDescriptor<T>, SearchDescriptor<T>> selectorWithoutContextAndFuzyy = new Func<SearchDescriptor<T>, SearchDescriptor<T>>(
                        sd => sd.Index(index).Suggest(s => s.Completion(completion_field, c => c.Field(completion_field)
                            .Fuzzy(ff => ff.MinLength(2).PrefixLength(0).Fuzziness(Fuzziness.EditDistance(1))).Prefix(searchTerm).Size(size))));
                    Func<SearchDescriptor<T>, SearchDescriptor<T>> selectorWithContextAndFuzyy = null;
                    if (context != null)
                    {
                        selectorWithContextAndFuzyy = new Func<SearchDescriptor<T>, SearchDescriptor<T>>(sd => sd.Index(index)
                            .Suggest(s => s.Completion(completion_field, c => c
                                .Field(completion_field)
                                .Fuzzy(ff => ff.MinLength(2).PrefixLength(0).Fuzziness(Fuzziness.EditDistance(1)))
                                .Size(size)
                                .Contexts(contentDict)
                                .Prefix(searchTerm))));
                    }
                    _result = client.Search<T>(context == null ? selectorWithoutContextAndFuzyy : selectorWithContextAndFuzyy);
                }
                var res = _result.Suggest[completion_field][0].Options.ToArray();
                _suggestionList = res.Cast<Nest.SuggestOption<T>>();
            }
            catch (Exception ex)
            {
                string msg = ("search term : " + searchTerm ?? "") + ",completion  field : " + (completion_field ?? "") + ", size : " + size + ",context : " + (context != null ? context.First() : "");
                Console.WriteLine(msg);
            }
            return _suggestionList;
        }
    }
}
