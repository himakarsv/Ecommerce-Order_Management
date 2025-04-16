using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.Attributes
{
    public class RedirectIfAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var email = context.HttpContext.Session.GetString("Email");
            if(!string.IsNullOrEmpty(email))
            {
                string role = context.HttpContext.Session.GetString("role");
                if (role == "Customer")
                {
                    context.Result = new RedirectToActionResult("Index", "Customer", null);
                }
                if (role == "Admin")
                {
                    context.Result = new RedirectToActionResult("Index", "Admin", null);
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
