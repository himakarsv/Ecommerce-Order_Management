//using Ecommerce.Attributes;
//using Ecommerce.Data;
//using Ecommerce.Interfaces;
//using Ecommerce.Models;
//using Microsoft.AspNetCore.Mvc;
//using Ecommerce.Attributes;
//using Microsoft.EntityFrameworkCore;
//using Ecommerce.Models.Enums;
//using Ecommerce.ViewModels;

//namespace Ecommerce.Controllers
//{
//    [RoleAuthorize("Customer")]
//    public class CustomerController : Controller
//    {
//        private readonly IProductRepository _db;
//        private readonly ApplicationDbContext _context;

//        public CustomerController(IProductRepository db,ApplicationDbContext context)
//        {
//            _db = db;
//            _context = context;
//        }
//        public async Task<IActionResult> Orders()
//        {
//             var email = HttpContext.Session.GetString("Email");
//            var user = await _context.Users.OfType<Customer>()
//                .Include(c => c.Orders)
//                .ThenInclude(o => o.Products)
//                .FirstOrDefaultAsync(c => c.Email == email);

//            if (user == null) return RedirectToAction("Login", "Account");

//            return View(user.Orders.OrderByDescending(o => o.CreatedDate).ToList());
//        }

//        public async Task<IActionResult> Index()
//        {
//            var products = await _db.GetAllAsync();
//            return View(products);
//        }

//        [HttpGet]
//        public async Task<IActionResult> BuyNow(int productID)
//        {
//            var product = await _context.Products.FindAsync(productID);
//            if (product == null) return NotFound();

//            TempData["ProductId"] = productID;
//            return View();
//        }
//        [HttpPost]
//        public async Task<IActionResult> BuyNow(PlaceOrderViewModel model)
//        {
//            if (!ModelState.IsValid) return View(model);


//            var email = HttpContext.Session.GetString("Email");
//            var customer = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
//            if (customer == null) return RedirectToAction("Login", "Account");

//            var productId = int.Parse(TempData["ProductId"].ToString());
//            var product = await _context.Products.FindAsync(productId);
//            if (product == null) return NotFound();

//            Order newOrder = new Order
//            {
//                CustomerId = customer.UserId,
//                CreatedDate = DateTime.Now,
//                Status = OrderStatus.Pending,
//                Products = new List<Product> { product },
//                Address = new Address
//                {
//                    State = model.State,
//                    City = model.City,
//                    PostalCode = model.PostalCode
//                }
//            };


//            await _context.Orders.AddAsync(newOrder);
//            await _context.SaveChangesAsync();

//            TempData["Message"] = "Order placed successfully!";
//            return RedirectToAction("Orders");

//            return View();
//        }




//    }
//}





using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Models.Enums;
using Ecommerce.ViewModels;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Controllers
{
    [RoleAuthorize("Customer")]
    public class CustomerController : Controller
    {
        private readonly IProductRepository _db;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(IProductRepository db, ApplicationDbContext context, ILogger<CustomerController> logger)
        {
            _db = db;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Orders()
        {
            try
            {
                var email = HttpContext.Session.GetString("Email");
                var user = await _context.Users.OfType<Customer>()
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.Products)
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("Attempt to access orders by unauthenticated user");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation("Customer {Email} viewed orders", email);
                return View(user.Orders.OrderByDescending(o => o.CreatedDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for customer");
                TempData["Error"] = "Could not load your orders. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _db.GetAllAsync();
                _logger.LogInformation("Loaded products for customer product listing page");
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products for customer");
                TempData["Error"] = "Unable to load products at the moment.";
                return RedirectToAction("Orders");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuyNow(int productID)
        {
            try
            {
                var product = await _context.Products.FindAsync(productID);
                if (product == null)
                {
                    _logger.LogWarning("BuyNow requested for invalid product ID {ProductId}", productID);
                    return NotFound();
                }

                TempData["ProductId"] = productID;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BuyNow GET for product ID {ProductId}", productID);
                TempData["Error"] = "Something went wrong while initiating purchase.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuyNow(PlaceOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var email = HttpContext.Session.GetString("Email");
                var customer = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
                if (customer == null)
                {
                    _logger.LogWarning("Unauthorized BuyNow POST attempt");
                    return RedirectToAction("Login", "Account");
                }

                var productId = int.Parse(TempData["ProductId"]?.ToString() ?? "0");
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("BuyNow POST received invalid product ID {ProductId}", productId);
                    return NotFound();
                }

                Order newOrder = new Order
                {
                    CustomerId = customer.UserId,
                    CreatedDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    Products = new List<Product> { product },
                    Address = new Address
                    {
                        State = model.State,
                        City = model.City,
                        PostalCode = model.PostalCode
                    }
                };

                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation("BuyNow: Order placed by {Email} for Product ID {ProductId}", email, productId);
                TempData["Message"] = "Order placed successfully!";
                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BuyNow POST failed");
                TempData["Error"] = "Failed to place order. Please try again.";
                return View(model);
            }
        }
    }
}
