using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Attributes;
using Ecommerce.ViewModels;


public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("", "Email already registered.");
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

            return RedirectToAction("Login");
        }

        return View(model);
    }

    // GET: /Account/Login

    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email"))){
            string role = HttpContext.Session.GetString("role");
            if (role == "Customer")
                return RedirectToAction("Index", "Customer");

            else if (role == "Admin")
                    return RedirectToAction("Index", "Admin");
                else
                return RedirectToAction("Login", "Account");
        }
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email"))){
            string role = HttpContext.Session.GetString("role");
            if (role == "Customer")
                return RedirectToAction("Index", "Customer");

            else if (role == "Admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Email", user.Email);


            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Customer");
        }

        return View(model);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

}
