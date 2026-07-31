using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;

        public ProductsModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            Products = await _productService.GetActiveProductsAsync();
        }
    }
}