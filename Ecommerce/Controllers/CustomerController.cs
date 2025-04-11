using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
