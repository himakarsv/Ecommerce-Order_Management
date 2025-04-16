using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.ViewModels;
using Ecommerce.Attributes;
using Ecommerce.Data;
using Microsoft.EntityFrameworkCore;
namespace Ecommerce.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository _db;
        private readonly ApplicationDbContext _context;

        public AdminController(IProductRepository db,ApplicationDbContext context)
        {
            _db = db;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.GetAllAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <= 0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");

            if (ModelState.IsValid)
            {
                await _db.AddAsync(p);
                await _db.SaveAsync();
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        public async Task<IActionResult> Edit(int? productID)
        {
            if (productID == 0 || productID == null) return NotFound();

            var product = await _db.GetAsync(u => u.ProductID == productID);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <= 0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");

            if (ModelState.IsValid)
            {
                if (p == null) return NotFound();
                _db.Update(p);
                await _db.SaveAsync();
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        public async Task<IActionResult> Delete(int? productID)
        {
            if (productID == 0 || productID == null) return NotFound();

            var product = await _db.GetAsync(u => u.ProductID == productID);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Product p)
        {
            if (p == null) return NotFound();

            _db.Remove(p);
            await _db.SaveAsync();
            return RedirectToAction("Index", "Admin");
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
           .Include(o => o.Customer)          
           .Include(o => o.Products)          
           .ToListAsync();
            return View(orders);
        }
    }
}
