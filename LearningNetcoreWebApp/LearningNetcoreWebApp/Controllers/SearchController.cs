using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LearningNetcoreWebApp.Models;

namespace LearningNetcoreWebApp.Controllers
{
    //[Route("search/main")]
    public class SearchController : Controller
    {        
        public ActionResult Index()
        {
            return View("~/Views/Search/Vehicle.cshtml");
        }
    }
}
