using EWasteSolutions.Models;
using EWasteSolutions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EWasteSolutions.Pages.Admin.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IImageService _imageService;

        public CreateModel(
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

        public SelectList CategoryList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ValidateImage();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadResult =
                        await _imageService.UploadAsync(ImageFile);

                    Product.ProductImages ??= new List<ProductImage>();

                    Product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = uploadResult.ImageUrl,
                        PublicId = uploadResult.PublicId,
                        IsPrimary = true
                    });
                }

                Product.CreatedAt = DateTime.UtcNow;
                Product.UpdatedAt = DateTime.UtcNow;

                await _productService.CreateAsync(Product);

                TempData["SuccessMessage"] =
                    "Product added successfully.";

                return RedirectToPage("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The product could not be saved. Please try again.");

                await LoadCategoriesAsync();

                return Page();
            }
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

            if (!ImageFile.ContentType.StartsWith(
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
            var categories =
                await _categoryService.GetAllAsync();

            CategoryList = new SelectList(
                categories.Where(c => c.IsActive),
                "Id",
                "Name",
                Product.CategoryId);
        }
    }
}