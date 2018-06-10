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
                ViewBag.Items = cart.items != null ? cart.items : new List<PhoneModel>();
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
