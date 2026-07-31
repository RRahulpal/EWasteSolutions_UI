using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EWasteSolutions.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IImageService _imageService;
        public DeleteModel(
     IProductService productService,
     IImageService imageService)
        {
            _productService = productService;
            _imageService = imageService;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetByIdAsync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                var imagePublicIds = product.ProductImages?
                    .Where(image => !string.IsNullOrWhiteSpace(image.PublicId))
                    .Select(image => image.PublicId)
                    .ToList() ?? new List<string>();

                await _productService.DeleteAsync(id);

                foreach (var publicId in imagePublicIds)
                {
                    try
                    {
                        await _imageService.DeleteAsync(publicId);
                    }
                    catch
                    {
                        // Product deletion should remain successful even if
                        // Cloudinary cleanup temporarily fails.
                    }
                }

                TempData["SuccessMessage"] =
                    "Product deleted successfully.";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"The product could not be deleted: {ex.Message}";

                return RedirectToPage("Index");
            }
        }
    }
}