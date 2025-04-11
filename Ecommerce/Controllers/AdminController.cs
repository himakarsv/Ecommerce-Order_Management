using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly IProductRepository _db;

        public AdminController(IProductRepository db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Product> products= (List<Product>)_db.GetAll();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <=0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");
            if (ModelState.IsValid)
            {
                _db.Add(p);
                _db.Save();
            return RedirectToAction("Index","Admin");
            }
            return View();

        }
        public IActionResult Edit(int? productID)
        {
            if (productID == 0 || productID == null) return NotFound();
            Product p = _db.Get(u => u.ProductID == productID);
            if (p == null) return NotFound();

            return View(p);
        }

        [HttpPost]
        public IActionResult Edit(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <= 0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");
            if (ModelState.IsValid)
            {
                if (p == null) return NotFound();
                _db.Update(p);
                _db.Save();
                return RedirectToAction("Index", "Admin");
            }
            return View();
            
        }

        public IActionResult Delete(int? productID)
        {
            if(productID == 0 || productID == null) { return NotFound(); }
            Product p = _db.Get(u => u.ProductID == productID);
            if (p == null) return NotFound();

            return View(p);
        }

        [HttpPost]
        public IActionResult Delete(Product p)
        {
            if(p == null) return NotFound();
            _db.Remove(p);
            _db.Save();
            return RedirectToAction("Index", "Admin");
        }

    }
}
