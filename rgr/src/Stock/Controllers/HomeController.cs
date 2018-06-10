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
        private static readonly Rabbit rabbit = Rabbit.GetInstance();

        [Route("")]
        public IActionResult Index()
        {
            var db = GetRealmInstance();
            var items = db.All<PhoneModel>().ToList();
            ViewBag.Items = items != null ? items : new List<PhoneModel>();
            return View();
        }

        [HttpGet("Remove/{id}")]
        public IActionResult Remove(string id)
        {
            try
            {
                var db = GetRealmInstance();
                var allItems = db.All<PhoneModel>().Where(i => i.id.Equals(id));
                var item = allItems.Count() > 0 ? allItems.First() : null;
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }

                rabbit.BroadcastEvent(Rabbit.EventType.ITEM_REMOVED, item);

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
            try
            {
                var db = GetRealmInstance();
                var allItems = db.All<PhoneModel>().Where(i => i.id.Equals(id));
                PhoneModel item = allItems.Count() > 0 ? allItems.First() : null;
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }
                ViewBag.Item = item;
                return View();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при поиске товара");
            }
        }

        [HttpPost("Edit/{id}")]
        public IActionResult Edit([FromForm] PhoneModel item)
        {
            try
            {
                var db = GetRealmInstance();
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
            rabbit.BroadcastEvent(Rabbit.EventType.ITEM_CHANGED, item);
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
                var db = GetRealmInstance();
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
            rabbit.BroadcastEvent(Rabbit.EventType.ITEM_ADDED, item);
            return View();
        }

        private Realm GetRealmInstance()
        {
            return Realm.GetInstance("stock");
        }
    }
}
