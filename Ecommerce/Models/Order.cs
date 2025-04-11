using System.ComponentModel.DataAnnotations;
using Ecommerce.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public List<Product> Products { get; set; } = new();
        public OrderStatus Status { get; set; }
        public Address? Address { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
