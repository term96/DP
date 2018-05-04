using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Frontend.Controllers
{
    public class StatisticsController : Controller
    {
        static readonly string BASE_ADDRESS = "http://localhost:5001";

        public IActionResult Index()
        {
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASE_ADDRESS);
                var request = client.GetAsync("api/values/statistics").Result;
                if (request.StatusCode == HttpStatusCode.OK)
                {
                    var response = request.Content.ReadAsStringAsync().Result;
                    string[] data = response.Split(';');
                    ViewData["TextNum"] = data[0];
					ViewData["HighRankPart"] = data[1];
					ViewData["AvgRank"] = data[2];
                    return View();
                }
                return NotFound("Не удалось получить данные с сервера");
            }
        }
    }
}
