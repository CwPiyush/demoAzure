using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningNetcoreWebApp
{
    public class ElasticClientOperations
    {
        ElasticClient client = null;
        public ElasticClientOperations()
        {
            var uri = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(uri);
            client = new ElasticClient(settings);            
        }        

        public static void CreateIndex<T>(string indexName, Func<MappingsDescriptor, IPromise<IMappings>> descriptor) where T : class
        {

            var numberOfShards = 2;
            var numberOfReplicas = 2;
            return client.CreateIndex(indexName,
                        ind => ind
                     .Settings(s => s.NumberOfShards(numberOfShards)
                         .NumberOfReplicas(numberOfReplicas)
                     )
                    .Mappings(descriptor));
        }

        public static ICreateIndexResponse CreateIndex<T>(ICreateIndexRequest request)
        {
            return client.CreateIndex(request);
        }

        public static IIndicesResponse DeleteIndex(string indexName)
        {
            return client.DeleteIndex(new DeleteIndexRequest(indexName));
        }

        public static IBulkAliasResponse Alias(Func<BulkAliasDescriptor, IBulkAliasRequest> aliasRequest)
        {
            return client.Alias(aliasRequest);
        }

        public static IBulkResponse AddDocument<T>(List<T> itemList, string indexName, Func<T, string> fieldSelector) where T : class
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
                log.Error(MethodBase.GetCurrentMethod().Name, ex);
            }
            return bulkResponse;
        }

        public static IIndexResponse AddDocument<T>(T item, string indexName, long id) where T : class
        {
            return client.Index<T>(item, ind => ind.Index(indexName).Id(id));
        }

        public static IIndexResponse AddDocument<T>(T item, string indexName, string id) where T : class
        {
            return client.Index<T>(item, ind => ind.Index(indexName).Id(id));
        }

        //public static ISuggestResponse Suggest<T>(string searchTerm, string indexName, string completionField, int size = 10, List<string> context = null, bool fuzzy = false) where T : class
        //{
        //    ISuggestResponse response;
        //    Func<FluentDictionary<string, object>, FluentDictionary<string, object>> contentDict = null;
        //    if (context != null)
        //    {
        //        contentDict = new Func<FluentDictionary<string, object>, FluentDictionary<string, object>>(cc => cc.Add("types", context));
        //        response = client.Suggest<T>(s => s.Index(indexName).GlobalText(searchTerm).Completion(completionField,
        //            sug => sug.Context(contentDict).OnField(completionField).Size(size)));
        //    }
        //    else
        //    {
        //        response = client.Suggest<T>(s => s.Index(indexName).GlobalText(searchTerm).Completion(completionField,
        //            sug => sug.OnField(completionField).Size(size)));
        //    }
        //    return response;
        //}



        public static bool DeleteDocument<T>(string Id, string indexName) where T : class
        {
            if (!string.IsNullOrEmpty(Id))
            {
                IDeleteResponse iResponse = client.Delete<T>(Id, d => d.Index(indexName));
                if (!iResponse.Found)
                {
                    log.Error("could not delete object with Id: " + Id);
                    return false;
                }
            }
            return true;
        }


    }
}
