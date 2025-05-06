using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.Enums;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Ecommerce.Controllers
{
    public class OrderController : Controller
    {

        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetOrderStatus()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PlaceOrder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = HttpContext.Session.GetString("Email");
            var user = await _context.Users.OfType<Customer>()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == user.UserId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Cart is empty.";
                return RedirectToAction("Index");
            }

            Order order = new Order
            {
                CustomerId = user.UserId,
                CreatedDate = DateTime.Now,
                Status = OrderStatus.Pending,
                Products = cartItems.Select(c => c.Product).ToList(),
            };
            order.Address = new Address
            {
                State = model.State,
                City = model.City,
                PostalCode = model.PostalCode
            };

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction("Orders", "Customer");
        }



       
    }
}
