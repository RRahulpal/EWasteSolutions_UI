using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;

        public DetailsModel(IProductService productService)
        {
            _productService = productService;
        }

        public Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product =
                await _productService.GetActiveProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            return Page();
        }
    }
}