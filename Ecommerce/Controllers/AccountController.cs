//using Ecommerce.Data;
//using Ecommerce.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Ecommerce.Attributes;
//using Ecommerce.ViewModels;


//public class AccountController : Controller
//{
//    private readonly ApplicationDbContext _context;

//    public AccountController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    // GET: /Account/Register
//    public IActionResult Register() => View();

//    // POST: /Account/Register
//    [HttpPost]
//    public async Task<IActionResult> Register(RegisterViewModel model)
//    {
//        if (ModelState.IsValid)
//        {
//            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
//            if (emailExists)
//            {
//                ModelState.AddModelError("", "Email already registered.");
//                return View(model);
//            }

//            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

//            User user = model.Role.ToLower() switch
//            {
//                "customer" => new Customer
//                {
//                    Name = model.Name,
//                    Email = model.Email,
//                    Password = hashedPassword,
//                    Role = "Customer"
//                },
//                "admin" => new Admin
//                {
//                    Name = model.Name,
//                    Email = model.Email,
//                    Password = hashedPassword,
//                    Role = "Admin"
//                },
//                _ => throw new ArgumentException("Invalid role")
//            };

//            await _context.Users.AddAsync(user);
//            await _context.SaveChangesAsync();

//            return RedirectToAction("Login");
//        }

//        return View(model);
//    }

//    // GET: /Account/Login

//    public IActionResult Login()
//    {
//        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email"))){
//            string role = HttpContext.Session.GetString("role");
//            if (role == "Customer")
//                return RedirectToAction("Index", "Customer");

//            else if (role == "Admin")
//                    return RedirectToAction("Index", "Admin");
//                else
//                return RedirectToAction("Login", "Account");
//        }
//        return View();
//    }

//    // POST: /Account/Login
//    [HttpPost]
//    public async Task<IActionResult> Login(LoginViewModel model)
//    {
//        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email"))){
//            string role = HttpContext.Session.GetString("role");
//            if (role == "Customer")
//                return RedirectToAction("Index", "Customer");

//            else if (role == "Admin")
//                return RedirectToAction("Index", "Admin");
//            else
//                return RedirectToAction("Login", "Account");
//        }

//        if (ModelState.IsValid)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
//            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
//            {
//                ModelState.AddModelError("", "Invalid email or password.");
//                return View(model);
//            }

//            HttpContext.Session.SetInt32("UserId", user.UserId);
//            HttpContext.Session.SetString("Role", user.Role);
//            HttpContext.Session.SetString("Email", user.Email);


//            return user.Role == "Admin"
//                ? RedirectToAction("Index", "Admin")
//                : RedirectToAction("Index", "Customer");
//        }

//        return View(model);
//    }

//    public IActionResult AccessDenied()
//    {
//        return View();
//    }

//    public IActionResult Logout()
//    {
//        HttpContext.Session.Clear();
//        return RedirectToAction("Login");
//    }

//}




using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Attributes;
using Ecommerce.ViewModels;
using Microsoft.Extensions.Logging;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Account/Register
    public IActionResult Register() {


        try
        {
            var email = HttpContext.Session.GetString("Email");
            if (!string.IsNullOrEmpty(email))
            {
                var role= HttpContext.Session.GetString("Role");
                _logger.LogInformation("User {Email} already logged in as {Role}", email, role);

                return role switch
                {
                    "Customer" => RedirectToAction("Index", "Customer"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Login")
                };
            }
        return View(); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Login GET request");
            return RedirectToAction("AccessDenied");
        }



    }


    public IActionResult Login()
    {
        try
        {
            var email = HttpContext.Session.GetString("Email");
            if (!string.IsNullOrEmpty(email))
            {
                var role = HttpContext.Session.GetString("Role");
                _logger.LogInformation("User {Email} already logged in as {Role}", email, role);

                return role switch
                {
                    "Customer" => RedirectToAction("Index", "Customer"),
                    "Admin" => RedirectToAction("Index", "Admin"),
                    _ => RedirectToAction("Login")
                };
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Login GET request");
            return RedirectToAction("AccessDenied");
        }
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        if (model.Role.ToLower() == "admin")
        {
            ModelState.AddModelError("", "Unauthorized role selection.");
            return View(model);
        }

        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already registered.");
                _logger.LogWarning("Registration attempt with existing email: {Email}", model.Email);
                return View(model);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            User user = model.Role.ToLower() switch
            {
                "customer" => new Customer
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = "Customer"
                },
                "admin" => new Admin
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = "Admin"
                },
                _ => throw new ArgumentException("Invalid role")
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New {Role} registered: {Email}", user.Role, user.Email);

            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during registration for {Email}", model.Email);
            ModelState.AddModelError("", "An error occurred while registering. Please try again.");
            return View(model);
        }
    }

   

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email")))
        {
            var role = HttpContext.Session.GetString("Role");
            return role switch
            {
                "Customer" => RedirectToAction("Index", "Customer"),
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Login")
            };
        }

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);

            _logger.LogInformation("User {Email} logged in as {Role}", user.Email, user.Role);

            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Customer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", model.Email);
            ModelState.AddModelError("", "An error occurred while logging in. Please try again.");
            return View(model);
        }
    }

    public IActionResult AccessDenied()
    {
        _logger.LogWarning("Access denied encountered.");
        return View();
    }

    public IActionResult Logout()
    {
        try
        {
            var email = HttpContext.Session.GetString("Email");
            HttpContext.Session.Clear();
            _logger.LogInformation("User {Email} logged out.", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout.");
        }

        return RedirectToAction("Login");
    }
}
