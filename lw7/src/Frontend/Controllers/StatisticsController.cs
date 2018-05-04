using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net;
using System.Net.Http;

namespace Frontend.Controllers
{
    public class StatisticsController : Controller
    {
        static readonly string BASE_ADDRESS = "http://localhost:5001";

        public IActionResult Index()
        {
            return View();
        }
    }
}
