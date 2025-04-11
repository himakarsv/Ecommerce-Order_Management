using Ecommerce.Models;

namespace Ecommerce.Interfaces
{
    public interface IProductRepository:IRepository<Product>
    {
        void Update(Product product);
        void Save();
    }
}
