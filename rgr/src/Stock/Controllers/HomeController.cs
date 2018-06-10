using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stock.Models;
using Realms;

namespace Stock.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            var db = Realm.GetInstance();
            var items = db.All<PhoneModel>().ToList();
            ViewBag.Items = items != null ? items : new List<PhoneModel>();
            return View();
        }

        [HttpGet("Remove/{id}")]
        public IActionResult Remove(string id)
        {
            try
            {
                var db = Realm.GetInstance();
                var item = db.All<PhoneModel>().Where(i => i.id.Equals(id)).First();
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }

                db.Write(() => 
                {
                    db.Remove(item);
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при удалении");
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public IActionResult Edit(string id)
        {
            PhoneModel item;
            try
            {
                var db = Realm.GetInstance();
                item = db.All<PhoneModel>().Where(i => i.id.Equals(id)).First();
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при поиске товара");
            }

            ViewBag.Item = item;
            return View();
        }

        [HttpPost("Edit/{id}")]
        public IActionResult Edit([FromForm] PhoneModel item)
        {
            try
            {
                var db = Realm.GetInstance();
                db.Write(() => 
                {
                    db.Add(item, update: true);
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                ViewBag.Result = "ERROR";
                return View();
            }
            
            ViewBag.Result = "SUCCESS";
            ViewBag.Item = item;
            return View();
        }

        [HttpGet("Add")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost("Add")]
        public IActionResult Add([FromForm] PhoneModel item)
        {
            item.id = Guid.NewGuid().ToString();
            try
            {
                var db = Realm.GetInstance();
                db.Write(() => 
                {
                    db.Add(item);
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                ViewBag.Result = "ERROR";
                return View();
            }
            
            ViewBag.Result = "SUCCESS";
            return View();
        }
    }
}
