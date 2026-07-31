using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EWasteSolutions.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;

        public EditModel(
            IProductService productService,
            ICategoryService categoryService,
            IImageService imageService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _imageService = imageService;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImageUrl { get; set; }

        public SelectList CategoryList { get; set; } = default!;


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

            CurrentImageUrl = product.ProductImages?
                .FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                ?? product.ProductImages?.FirstOrDefault()?.ImageUrl;

            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ValidateImage();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                await LoadCurrentImageAsync();

                return Page();
            }

            var existingProduct =
                await _productService.GetByIdAsync(Product.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            try
            {
                existingProduct.Name = Product.Name;
                existingProduct.Brand = Product.Brand;
                existingProduct.Price = Product.Price;
                existingProduct.Description = Product.Description;
                existingProduct.Processor = Product.Processor;
                existingProduct.Ram = Product.Ram;
                existingProduct.Storage = Product.Storage;
                existingProduct.Condition = Product.Condition;
                existingProduct.Warranty = Product.Warranty;
                existingProduct.Stock = Product.Stock;
                existingProduct.IsFeatured = Product.IsFeatured;
                existingProduct.IsActive = Product.IsActive;
                existingProduct.CategoryId = Product.CategoryId;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var oldPrimaryImage =
                        existingProduct.ProductImages?
                            .FirstOrDefault(image => image.IsPrimary)
                        ?? existingProduct.ProductImages?.FirstOrDefault();

                    var uploadResult =
                        await _imageService.UploadAsync(ImageFile);

                    existingProduct.ProductImages ??= new List<ProductImage>();

                    if (oldPrimaryImage != null)
                    {
                        var oldPublicId = oldPrimaryImage.PublicId;

                        oldPrimaryImage.ImageUrl = uploadResult.ImageUrl;
                        oldPrimaryImage.PublicId = uploadResult.PublicId;
                        oldPrimaryImage.IsPrimary = true;

                        if (!string.IsNullOrWhiteSpace(oldPublicId))
                        {
                            await _imageService.DeleteAsync(oldPublicId);
                        }
                    }
                    else
                    {
                        existingProduct.ProductImages.Add(new ProductImage
                        {
                            ImageUrl = uploadResult.ImageUrl,
                            PublicId = uploadResult.PublicId,
                            IsPrimary = true
                        });
                    }
                }

                await _productService.UpdateAsync(existingProduct);

                TempData["SuccessMessage"] =
                    "Product updated successfully.";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"The product could not be updated: {ex.Message}");

                await LoadCategoriesAsync();

                CurrentImageUrl = existingProduct.ProductImages?
                    .FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                    ?? existingProduct.ProductImages?.FirstOrDefault()?.ImageUrl;

                return Page();
            }
        }
        private async Task LoadCurrentImageAsync()
        {
            var existingProduct =
                await _productService.GetByIdAsync(Product.Id);

            CurrentImageUrl = existingProduct?.ProductImages?
                .FirstOrDefault(image => image.IsPrimary)?.ImageUrl
                ?? existingProduct?.ProductImages?.FirstOrDefault()?.ImageUrl;
        }

        private void ValidateImage()
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return;
            }

            var allowedExtensions = new[]
            {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

            var extension =
                Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            const long maximumFileSize = 5 * 1024 * 1024;

            if (ImageFile.Length > maximumFileSize)
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "The image size cannot exceed 5 MB.");
            }

            if (string.IsNullOrWhiteSpace(ImageFile.ContentType) ||
                !ImageFile.ContentType.StartsWith(
                    "image/",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Please select a valid image file.");
            }
        }
        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetAllAsync();

            CategoryList = new SelectList(
                categories,
                "Id",
                "Name",
                Product.CategoryId
            );
        }
    }
}