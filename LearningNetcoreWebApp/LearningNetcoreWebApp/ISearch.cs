using Models.LearningNetcoreWebApp;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LearningNetcoreWebApp
{
    public interface ISearch
    {            
        bool AddNewIndex(make model);
        object GetResult(int year, string term);
        List<Suggest> GetCarSuggestion(NameValueCollection nvc);
        bool CreateIndex();
    }
}