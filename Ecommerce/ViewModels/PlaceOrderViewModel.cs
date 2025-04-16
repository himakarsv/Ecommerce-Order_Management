using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class PlaceOrderViewModel
    {
        [Required]
        public string State { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PostalCode { get; set; }
    }
}
