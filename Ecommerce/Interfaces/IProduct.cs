namespace Ecommerce.Interfaces
{
    public interface IProduct
    {
        int ProductID { get; set; }
        string Name { get; set; }
        decimal Price { get; set; }

        void DisplayProductInfo();
    }
}
