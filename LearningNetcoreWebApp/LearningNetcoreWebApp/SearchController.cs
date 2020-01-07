using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearningNetcoreWebApp
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearch _iSearch;
        public SearchController(ISearch search)
        {
            _iSearch = search;
        }        

        // GET api/<controller>/5
        [HttpGet("{year}/cars/{term}")]
        public async Task<IActionResult> Get(int year, string term)
        {
            var result = _iSearch.GetResult(year, term);            
            return Ok(result);
        }

        [HttpGet, Route("autocomplete/car/")]
        public async Task<IActionResult> GetCarSuggestions()
        {

            NameValueCollection nvc = HttpUtility.ParseQueryString(Request.QueryString.Value);

            var citySuggestion = _iSearch.GetCarSuggestion(nvc);

            return Ok(citySuggestion);
        }

        [HttpGet("create/index")]
        public async Task<IActionResult> CreateIndex()
        {
            var result = _iSearch.CreateIndex();
            return Ok(result);
        }
    }
}
