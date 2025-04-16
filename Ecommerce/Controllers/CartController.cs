using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.Enums;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    [RoleAuthorize("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Cart/Add/5
        public async Task<IActionResult> Add(int productID)
        {
            // var email = HttpContext.Session.GetString("Email");
            //Console.WriteLine(email);
            var email = HttpContext.Session.GetString("Email");
            var user = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == user.UserId && c.ProductId == productID);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity++;
            }
            else
            {
                var cartItem = new Cart
                {
                    CustomerId = user.UserId,
                    ProductId = productID,
                    Quantity = 1
                };
                await _context.Carts.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Customer"); // or Cart view
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
             var email = HttpContext.Session.GetString("Email");
            var user = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == user.UserId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /Cart/Remove/5
        public async Task<IActionResult> Remove(int cartId)
        {
            var item = await _context.Carts.FindAsync(cartId);
            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> IncreaseQuantity(int cartId)
        {
            var item = await _context.Carts.FindAsync(cartId);
            if (item != null)
            {
                item.Quantity++;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DecreaseQuantity(int cartId)
        {
            var item = await _context.Carts.FindAsync(cartId);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        

    }
}
