using System.ComponentModel.DataAnnotations;
namespace Ecommerce.ViewModels 
{ 
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
}
}
