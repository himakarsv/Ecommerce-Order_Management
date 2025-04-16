using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Interfaces;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Attributes;
using Microsoft.EntityFrameworkCore;

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
    }
}
