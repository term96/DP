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
    public class HomeController : Controller
    {
        static readonly string BASE_ADDRESS = "http://localhost:5001";
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TextDetails(string id)
        {
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASE_ADDRESS);
                var request = client.GetAsync("api/values/status/" + id).Result;
                if (request.StatusCode == HttpStatusCode.OK)
                {
                    string resp = request.Content.ReadAsStringAsync().Result;
                    if (resp.StartsWith("done"))
                    {
                        string[] data = resp.Split(';');
                        ViewData["Status"] = data[0];
                        ViewData["Rank"] = data[1];
                    }
                    else
                    {
                        ViewData["Status"] = resp;
                    }

                    return View();
                }
                return NotFound("Текст не найден, попробуйте обновить страницу");
            }
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(string data)
        {
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BASE_ADDRESS);
                var content = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("value", data)
                });

                var request = client.PostAsync("api/values", content);

                if (request.Result.StatusCode == HttpStatusCode.OK)
                {
                    var requestContent = request.Result.Content.ReadAsStringAsync();
                    string id = requestContent.Result;
                    return RedirectToAction("TextDetails", new { id = id });
                }

                return Error();
            }
        }
    }
}
