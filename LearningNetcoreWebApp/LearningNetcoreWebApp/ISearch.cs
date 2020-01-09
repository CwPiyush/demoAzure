using Models.LearningNetcoreWebApp;
using Nest;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LearningNetcoreWebApp
{
    public interface ISearch
    {            
        bool AddNewIndex(make model);
        IEnumerable<SuggestOption<CarList>> GetResult(int year, string term);
        List<Suggest> GetCarSuggestion(NameValueCollection nvc);
        bool CreateIndex();
    }
}