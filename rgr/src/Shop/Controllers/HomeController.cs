using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shop.Models;
using Realms;

namespace Shop.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            try
            {
                var db = GetRealmInstance();
                var items = db.All<PhoneModel>().OrderBy((item) => item.price).ToList();
                ViewBag.Items = items != null ? items : new List<PhoneModel>();
                return View();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при получении списка товаров");
            }
        }

        [Route("MakeOrder/{count}")]
        public IActionResult MakeOrder(int count, [FromForm] OrderModel order)
        {
            try
            {
                var db = GetRealmInstance();
                var cart = GetOrCreateCart(db);
                foreach (var item in cart.items)
                {
                    order.items.Add(new PhoneModel() { id = Guid.NewGuid().ToString(), vendor = item.vendor, model = item.model, price = item.price });
                }
                order.id = Guid.NewGuid().ToString();
                order.status = "Не обработано";
                db.Write(() =>
                {
                    db.Add(order);
                    db.Remove(cart);
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при оформлении заказа");
            }
            return RedirectToAction("Cart");
        }

        [HttpGet("Orders")]
        public IActionResult Orders()
        {
            try
            {
                var db = GetRealmInstance();
                var orders = db.All<OrderModel>();

                ViewBag.Orders = orders.Count() > 0 ? orders.ToList() : new List<OrderModel>();
                return View();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при загрузке списка заказов");
            }
        }


        [Route("Details/{id}")]
        public IActionResult Details(string id)
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
                ViewBag.Item = item;
                return View();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при поиске товара");
            }
        }

        [Route("AddToCart/{id}")]
        public IActionResult AddToCart(string id)
        {
            try
            {
                var db = GetRealmInstance();
                var cart = GetOrCreateCart(db);
                var allItems = db.All<PhoneModel>().Where(i => i.id.Equals(id));
                var item = allItems.Count() > 0 ? allItems.First() : null;
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }
                db.Write(() =>
                {
                    cart.items.Add(item);
                });
                return RedirectToAction("Cart");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при добавлении товара в корзину");
            }
        }

        [Route("RemoveFromCart/{id}")]
        public IActionResult RemoveFromCart(string id)
        {
            try
            {
                var db = GetRealmInstance();
                var cart = GetOrCreateCart(db);
                var allItems = db.All<PhoneModel>().Where(i => i.id.Equals(id));
                var item = allItems.Count() > 0 ? allItems.First() : null;
                if (item == null)
                {
                    return NotFound("Указанный товар не найден");
                }
                db.Write(() =>
                {
                    cart.items.Remove(item);
                });
                return RedirectToAction("Cart");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при добавлении товара в корзину");
            }
        }

        [HttpGet("Cart")]
        public IActionResult Cart()
        {
            try
            {
                var db = GetRealmInstance();
                var cart = GetOrCreateCart(db);
                IList<PhoneModel> items = cart.items != null ? cart.items : new List<PhoneModel>();
                ViewBag.Items = items;
                ViewBag.Sum = items.Sum(i => { return i.price; });
                return View();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound("Ошибка при загрузке корзины");
            }
        }

        private CartModel GetOrCreateCart(Realm db)
        {
            var allCarts = db.All<CartModel>();
            var cart = allCarts.Count() > 0 ? allCarts.First() : null;
            if (cart == null)
            {
                cart = new CartModel();
                db.Write(() =>
                {
                    db.Add(cart);
                });
            }
            return cart;
        }

        private Realm GetRealmInstance()
        {
            return Realm.GetInstance("shop");
        }
    }
}
