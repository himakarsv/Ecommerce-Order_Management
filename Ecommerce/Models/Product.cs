using Ecommerce.Interfaces;

namespace Ecommerce.Models
{
    public class Product : IProduct
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public void DisplayProductInfo()
        {
            Console.WriteLine($"{ProductID} - {Name} Price - {Price}");
        }
    }
}
