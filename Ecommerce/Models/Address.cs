using Microsoft.EntityFrameworkCore;
namespace Ecommerce.Models
{
    [Owned]
    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
    }
}
