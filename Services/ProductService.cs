using EWasteSolutions.Data;
using EWasteSolutions.Models;
using Microsoft.EntityFrameworkCore;

namespace EWasteSolutions.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
        .AsNoTracking()
        .Include(product => product.Category)
        .Include(product => product.ProductImages)
        .OrderByDescending(product => product.CreatedAt)
        .ToListAsync();
        }

        public async Task<List<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Include(product => product.ProductImages)
                .Where(product => product.IsActive)
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.CreatedAt)
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == id);
        }
        public async Task<Product?> GetActiveProductByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Include(product => product.ProductImages)
                .FirstOrDefaultAsync(product =>
                    product.Id == id &&
                    product.IsActive);
        }
    }
}