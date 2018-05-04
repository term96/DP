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
using Microsoft.Extensions.Caching.Memory;

namespace Frontend.Controllers
{
    public class StatisticsController : Controller
    {
        static readonly string BASE_ADDRESS = "http://localhost:5001";
        static readonly String CACHE_KEY = "StatisticsCache";
        static readonly double CACHE_LIFETIME_SECONDS = 20;
        readonly IMemoryCache _cache;
        public StatisticsController(IMemoryCache cache)
        {
            _cache = cache;
        }
        public IActionResult Index()
        {
            String data;
            if (_cache.TryGetValue(CACHE_KEY, out data))
            {
                Console.WriteLine("Statistics loaded from cache");
            }
            else
            {
                using(HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BASE_ADDRESS);
                    var request = client.GetAsync("api/values/statistics").Result;
                    if (request.StatusCode != HttpStatusCode.OK)
                    {
                        return NotFound("Не удалось получить данные с сервера");
                    }
                    data = request.Content.ReadAsStringAsync().Result;
                    _cache.Set(CACHE_KEY, data, DateTimeOffset.Now.AddSeconds(CACHE_LIFETIME_SECONDS));
                }
                Console.WriteLine("Statistics loaded from backend");
            }
            string[] values = data.Split(';');
            ViewData["TextNum"] = values[0];
			ViewData["HighRankPart"] = values[1];
		    ViewData["AvgRank"] = values[2];
            return View();
        }
    }
}
