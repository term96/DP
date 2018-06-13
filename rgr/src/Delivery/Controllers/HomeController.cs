using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Delivery.Models;
using Realms;

namespace Delivery.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost("ChangeStatus/{id}")]
        public IActionResult ChangeStatus(string id, [FromForm] string status)
        {
            var db = GetRealmInstance();
            var items = db.All<OrderModel>().Where(i => i.id.Equals(id)).ToList();
            if (items.Count() == 0)
            {
                return NotFound("Заказ не найден");
            }
            var item = items[0];
            try
            {
                db.Write(() =>
                {
                    item.status = status;
                    Rabbit.GetInstance().BroadcastOrderChangedEvent(item);
                    if (status.Equals("Отменён"))
                    {
                        db.Remove(item);
                    }
                    else
                    {
                        db.Add(item, update: true);
                    }
                });
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при смене статуса заказа");
            }
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var db = GetRealmInstance();
            var items = db.All<OrderModel>().ToList();
            ViewBag.Orders = items.Count > 0 ? items : new List<OrderModel>();
            return View();
        }

        private Realm GetRealmInstance()
        {
            return Realm.GetInstance("delivery");
        }
    }
}
