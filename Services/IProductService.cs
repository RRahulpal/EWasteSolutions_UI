using EWasteSolutions.Models;

namespace EWasteSolutions.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task CreateAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
        Task<List<Product>> GetActiveProductsAsync();
        Task<Product?> GetActiveProductByIdAsync(int id);
    }
}