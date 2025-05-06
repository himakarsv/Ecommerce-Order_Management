using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Attributes;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Models.Enums;
using Ecommerce.ViewModels;

namespace Ecommerce.Controllers
{
    [RoleAuthorize("Customer")]
    public class CustomerController : Controller
    {
        private readonly IProductRepository _db;
        private readonly ApplicationDbContext _context;

        public CustomerController(IProductRepository db,ApplicationDbContext context)
        {
            _db = db;
            _context = context;
        }
        public async Task<IActionResult> Orders()
        {
             var email = HttpContext.Session.GetString("Email");
            var user = await _context.Users.OfType<Customer>()
                .Include(c => c.Orders)
                .ThenInclude(o => o.Products)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (user == null) return RedirectToAction("Login", "Account");

            return View(user.Orders.OrderByDescending(o => o.CreatedDate).ToList());
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.GetAllAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> BuyNow(int productID)
        {
            var product = await _context.Products.FindAsync(productID);
            if (product == null) return NotFound();

            TempData["ProductId"] = productID;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> BuyNow(PlaceOrderViewModel model)
        {
            if (!ModelState.IsValid) return View(model);


            var email = HttpContext.Session.GetString("Email");
            var customer = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
            if (customer == null) return RedirectToAction("Login", "Account");

            var productId = int.Parse(TempData["ProductId"].ToString());
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

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

            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction("Orders");

            return View();
        }




    }
}
