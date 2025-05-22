//using Ecommerce.Interfaces;
//using Ecommerce.Models;
//using Microsoft.AspNetCore.Mvc;
//using Ecommerce.ViewModels;
//using Ecommerce.Attributes;
//using Ecommerce.Data;
//using Microsoft.EntityFrameworkCore;
//namespace Ecommerce.Controllers
//{
//    [RoleAuthorize("Admin")]
//    public class AdminController : Controller
//    {
//        private readonly IProductRepository _db;
//        private readonly ApplicationDbContext _context;

//        public AdminController(IProductRepository db,ApplicationDbContext context)
//        {
//            _db = db;
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var products = await _db.GetAllAsync();
//            return View(products);
//        }

//        public IActionResult Create()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(Product p)
//        {
//            if (p.Name == null)
//                ModelState.AddModelError("Name", "Product Name cannot be empty");
//            if (p.Price <= 0)
//                ModelState.AddModelError("Price", "Product Price is Invalid.");

//            if (ModelState.IsValid)
//            {
//                await _db.AddAsync(p);
//                await _db.SaveAsync();
//                return RedirectToAction("Index", "Admin");
//            }

//            return View();
//        }

//        public async Task<IActionResult> Edit(int? productID)
//        {
//            if (productID == 0 || productID == null) return NotFound();

//            var product = await _db.GetAsync(u => u.ProductID == productID);
//            if (product == null) return NotFound();

//            return View(product);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(Product p)
//        {
//            if (p.Name == null)
//                ModelState.AddModelError("Name", "Product Name cannot be empty");
//            if (p.Price <= 0)
//                ModelState.AddModelError("Price", "Product Price is Invalid.");

//            if (ModelState.IsValid)
//            {
//                if (p == null) return NotFound();
//                _db.Update(p);
//                await _db.SaveAsync();
//                return RedirectToAction("Index", "Admin");
//            }

//            return View();
//        }

//        public async Task<IActionResult> Delete(int? productID)
//        {
//            if (productID == 0 || productID == null) return NotFound();

//            var product = await _db.GetAsync(u => u.ProductID == productID);
//            if (product == null) return NotFound();

//            return View(product);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Delete(Product p)
//        {
//            if (p == null) return NotFound();

//            _db.Remove(p);
//            await _db.SaveAsync();
//            return RedirectToAction("Index", "Admin");
//        }

//        public async Task<IActionResult> Orders()
//        {
//            var orders = await _context.Orders
//           .Include(o => o.Customer)          
//           .Include(o => o.Products)          
//           .ToListAsync();
//            return View(orders);
//        }
//    }
//}


using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.ViewModels;
using Ecommerce.Attributes;
using Ecommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Controllers
{
    [RoleAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly IProductRepository _db;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IProductRepository db, ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _db = db;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _db.GetAllAsync();
                _logger.LogInformation("Fetched {Count} products for admin view", products.Count());
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load product list on Admin/Index");
                TempData["Error"] = "Unable to load products. Please try again later.";
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if(await _context.Users.AnyAsync(u=>u.Email==model.Email) )
            {
                ModelState.AddModelError("", "User already exists");
                return View(model);
            }

            string hashPassword= BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newAdmin = new Admin()
            {
                Name = model.Name,
                Email = model.Email,
                Password = hashPassword,
                Role="Admin",
            };

            await _context.Users.AddAsync(newAdmin);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Admin Created Successfully";

            return RedirectToAction("Index","Admin");
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <= 0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");

            if (!ModelState.IsValid)
                return View();

            try
            {
                await _db.AddAsync(p);
                await _db.SaveAsync();
                _logger.LogInformation("Product created: {ProductName} at ₹{Price}", p.Name, p.Price);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product: {ProductName}", p.Name);
                TempData["Error"] = "Product creation failed.";
                return View(p);
            }
        }

        public async Task<IActionResult> Edit(int? productID)
        {
            if (productID == 0 || productID == null)
                return NotFound();

            try
            {
                var product = await _db.GetAsync(u => u.ProductID == productID);
                if (product == null)
                {
                    _logger.LogWarning("Attempt to edit non-existing product with ID: {ProductId}", productID);
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for editing with ID: {ProductId}", productID);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product p)
        {
            if (p.Name == null)
                ModelState.AddModelError("Name", "Product Name cannot be empty");
            if (p.Price <= 0)
                ModelState.AddModelError("Price", "Product Price is Invalid.");

            if (!ModelState.IsValid)
                return View(p);

            try
            {
                _db.Update(p);
                await _db.SaveAsync();
                _logger.LogInformation("Product updated: {ProductId}", p.ProductID);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", p.ProductID);
                TempData["Error"] = "Failed to update product.";
                return View(p);
            }
        }

        public async Task<IActionResult> Delete(int? productID)
        {
            if (productID == 0 || productID == null)
                return NotFound();

            try
            {
                var product = await _db.GetAsync(u => u.ProductID == productID);
                if (product == null)
                {
                    _logger.LogWarning("Attempt to delete non-existing product with ID: {ProductId}", productID);
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load product for delete with ID: {ProductId}", productID);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Product p)
        {
            if (p == null) return NotFound();

            try
            {
                _db.Remove(p);
                await _db.SaveAsync();
                _logger.LogInformation("Product deleted: {ProductId}", p.ProductID);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", p.ProductID);
                TempData["Error"] = "Product deletion failed.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Orders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Products)
                    .ToListAsync();

                _logger.LogInformation("Admin viewed all orders. Count: {OrderCount}", orders.Count);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders list for admin");
                TempData["Error"] = "Unable to load orders.";
                return RedirectToAction("Index");
            }
        }
    }
}
