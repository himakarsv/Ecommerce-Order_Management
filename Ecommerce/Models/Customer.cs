namespace Ecommerce.Models
{
    public class Customer : User
    {

        public List<Order> Orders = new();
        public override void DisplayUserInfo()
        {
            Console.WriteLine($"User=Customer Name - {Name}");
        }
        public void PlaceOrder() { }
    }
}
