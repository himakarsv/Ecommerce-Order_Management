namespace Ecommerce.Models
{
    public class Admin : User
    {
        public override void DisplayUserInfo()
        {
            Console.WriteLine($"User=Admin  Name - {Name}");
        }
        public void ManageInventory() { }
    }
}
