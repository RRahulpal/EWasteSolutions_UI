using EWasteSolutions.Models;

namespace EWasteSolutions.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int id);

        Task CreateAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}