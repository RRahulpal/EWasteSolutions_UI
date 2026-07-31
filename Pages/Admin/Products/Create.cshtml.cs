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
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IProductService productService,
            ICategoryService categoryService,
            IImageService imageService,
            ILogger<CreateModel> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _imageService = imageService;
            _logger = logger;
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
            // Explicit fallback for file binding.
            ImageFile ??= Request.Form.Files.GetFile(nameof(ImageFile));

            ValidateImage();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            string? uploadedPublicId = null;

            try
            {
                if (ImageFile == null || ImageFile.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(ImageFile),
                        "No image file was received by the server.");

                    await LoadCategoriesAsync();
                    return Page();
                }

                _logger.LogInformation(
                    "Uploading image {FileName}, size {FileSize}, type {ContentType}",
                    ImageFile.FileName,
                    ImageFile.Length,
                    ImageFile.ContentType);

                var uploadResult =
                    await _imageService.UploadAsync(ImageFile);

                if (string.IsNullOrWhiteSpace(uploadResult.ImageUrl) ||
                    string.IsNullOrWhiteSpace(uploadResult.PublicId))
                {
                    throw new InvalidOperationException(
                        "Cloudinary did not return an image URL or public ID.");
                }

                uploadedPublicId = uploadResult.PublicId;

                Product.ProductImages ??= new List<ProductImage>();

                Product.ProductImages.Add(new ProductImage
                {
                    ImageUrl = uploadResult.ImageUrl,
                    PublicId = uploadResult.PublicId,
                    AltText = Product.Name,
                    IsPrimary = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                });

                Product.CreatedAt = DateTime.UtcNow;
                Product.UpdatedAt = DateTime.UtcNow;

                await _productService.CreateAsync(Product);

                TempData["SuccessMessage"] =
                    "Product and image added successfully.";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Product image upload or product creation failed.");

                // If Cloudinary upload succeeded but database saving failed,
                // remove the orphaned Cloudinary image.
                if (!string.IsNullOrWhiteSpace(uploadedPublicId))
                {
                    try
                    {
                        await _imageService.DeleteAsync(uploadedPublicId);
                    }
                    catch (Exception cleanupException)
                    {
                        _logger.LogError(
                            cleanupException,
                            "Failed to remove orphaned Cloudinary image {PublicId}.",
                            uploadedPublicId);
                    }
                }

                ModelState.AddModelError(
                    string.Empty,
                    $"Unable to save the product: {ex.Message}");

                await LoadCategoriesAsync();
                return Page();
            }
        }

        private void ValidateImage()
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Please select a product image.");

                return;
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path
                .GetExtension(ImageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            const long maximumFileSize = 10 * 1024 * 1024;

            if (ImageFile.Length > maximumFileSize)
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "The image size cannot exceed 10 MB.");
            }

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (string.IsNullOrWhiteSpace(ImageFile.ContentType) ||
                !allowedContentTypes.Contains(
                    ImageFile.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "The selected file is not a supported image.");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categories =
                await _categoryService.GetAllAsync();

            CategoryList = new SelectList(
                categories.Where(category => category.IsActive),
                "Id",
                "Name",
                Product.CategoryId);
        }
    }
}