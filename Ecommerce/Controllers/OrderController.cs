using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
