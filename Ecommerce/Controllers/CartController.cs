//using Ecommerce.Attributes;
//using Ecommerce.Data;
//using Ecommerce.Models;
//using Ecommerce.Models.Enums;
//using Ecommerce.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Ecommerce.Controllers
//{
//    [RoleAuthorize("Customer")]
//    public class CartController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public CartController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // POST: /Cart/Add/5
//        public async Task<IActionResult> Add(int productID)
//        {
//            // var email = HttpContext.Session.GetString("Email");
//            //Console.WriteLine(email);
//            var email = HttpContext.Session.GetString("Email");
//            var user = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
//            if (user == null) return RedirectToAction("Login", "Account");

//            var existingCartItem = await _context.Carts
//                .FirstOrDefaultAsync(c => c.CustomerId == user.UserId && c.ProductId == productID);

//            if (existingCartItem != null)
//            {
//                existingCartItem.Quantity++;
//            }
//            else
//            {
//                var cartItem = new Cart
//                {
//                    CustomerId = user.UserId,
//                    ProductId = productID,
//                    Quantity = 1
//                };
//                await _context.Carts.AddAsync(cartItem);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction("Index", "Customer"); // or Cart view
//        }

//        // GET: /Cart
//        public async Task<IActionResult> Index()
//        {
//             var email = HttpContext.Session.GetString("Email");
//            var user = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
//            if (user == null) return RedirectToAction("Login", "Account");

//            var cartItems = await _context.Carts
//                .Include(c => c.Product)
//                .Where(c => c.CustomerId == user.UserId)
//                .ToListAsync();

//            return View(cartItems);
//        }

//        // POST: /Cart/Remove/5
//        public async Task<IActionResult> Remove(int cartId)
//        {
//            var item = await _context.Carts.FindAsync(cartId);
//            if (item != null)
//            {
//                _context.Carts.Remove(item);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction("Index");
//        }

//        public async Task<IActionResult> IncreaseQuantity(int cartId)
//        {
//            var item = await _context.Carts.FindAsync(cartId);
//            if (item != null)
//            {
//                item.Quantity++;
//                await _context.SaveChangesAsync();
//            }
//            return RedirectToAction("Index");
//        }

//        public async Task<IActionResult> DecreaseQuantity(int cartId)
//        {
//            var item = await _context.Carts.FindAsync(cartId);
//            if (item != null && item.Quantity > 1)
//            {
//                item.Quantity--;
//                await _context.SaveChangesAsync();
//            }
//            return RedirectToAction("Index");
//        }



//    }
//}


using Ecommerce.Attributes;
using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Models.Enums;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Controllers
{
    [RoleAuthorize("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: /Cart/Add/5
        public async Task<IActionResult> Add(int productID)
        {
            try
            {
                var email = HttpContext.Session.GetString("Email");
                var user = await _context.Users.OfType<Customer>().FirstOrDefaultAsync(u => u.Email == email);
                if (user == null) return RedirectToAction("Login", "Account");

                var existingCartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CustomerId == user.UserId && c.ProductId == productID);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                    _logger.LogInformation("Increased quantity of product {ProductId} in cart for user {Email}", productID, email);
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
                    _logger.LogInformation("Added product {ProductId} to cart for user {Email}", productID, email);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Item added to cart.";
                return RedirectToAction("Index", "Customer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding product {ProductId} to cart", productID);
                TempData["Error"] = "Failed to add item to cart.";
                return RedirectToAction("Index", "Customer");
            }
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading cart page");
                TempData["Error"] = "Unable to load your cart. Please try again.";
                return RedirectToAction("Index", "Customer");
            }
        }

        // POST: /Cart/Remove/5
        public async Task<IActionResult> Remove(int cartId)
        {
            try
            {
                var item = await _context.Carts.FindAsync(cartId);
                if (item != null)
                {
                    _context.Carts.Remove(item);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Removed item {CartId} from cart", cartId);
                    TempData["Message"] = "Item removed from cart.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartId}", cartId);
                TempData["Error"] = "Failed to remove item from cart.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> IncreaseQuantity(int cartId)
        {
            try
            {
                var item = await _context.Carts.FindAsync(cartId);
                if (item != null)
                {
                    item.Quantity++;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Increased quantity of cart item {CartId}", cartId);
                    TempData["Message"] = "Item quantity increased.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error increasing quantity for cart item {CartId}", cartId);
                TempData["Error"] = "Failed to increase quantity.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DecreaseQuantity(int cartId)
        {
            try
            {
                var item = await _context.Carts.FindAsync(cartId);
                if (item != null && item.Quantity > 1)
                {
                    item.Quantity--;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Decreased quantity of cart item {CartId}", cartId);
                    TempData["Message"] = "Item quantity decreased.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decreasing quantity for cart item {CartId}", cartId);
                TempData["Error"] = "Failed to decrease quantity.";
            }

            return RedirectToAction("Index");
        }
    }
}
