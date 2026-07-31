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
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IProductService productService,
            ICategoryService categoryService,
            IImageService imageService,
            ILogger<EditModel> logger)
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

            SetCurrentImage(product);

            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Fallback if normal file binding fails.
            ImageFile ??= Request.Form.Files.GetFile(nameof(ImageFile));

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

            string? newlyUploadedPublicId = null;
            string? oldPublicId = null;

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
                    _logger.LogInformation(
                        "Replacing product image. File: {FileName}, Size: {FileSize}",
                        ImageFile.FileName,
                        ImageFile.Length);

                    var uploadResult =
                        await _imageService.UploadAsync(ImageFile);

                    if (string.IsNullOrWhiteSpace(uploadResult.ImageUrl) ||
                        string.IsNullOrWhiteSpace(uploadResult.PublicId))
                    {
                        throw new InvalidOperationException(
                            "Cloudinary did not return valid image details.");
                    }

                    newlyUploadedPublicId = uploadResult.PublicId;

                    existingProduct.ProductImages ??=
                        new List<ProductImage>();

                    var oldPrimaryImage =
                        existingProduct.ProductImages
                            .FirstOrDefault(image => image.IsPrimary)
                        ?? existingProduct.ProductImages.FirstOrDefault();

                    if (oldPrimaryImage != null)
                    {
                        oldPublicId = oldPrimaryImage.PublicId;

                        oldPrimaryImage.ImageUrl =
                            uploadResult.ImageUrl;

                        oldPrimaryImage.PublicId =
                            uploadResult.PublicId;

                        oldPrimaryImage.AltText =
                            existingProduct.Name;

                        oldPrimaryImage.IsPrimary = true;

                        oldPrimaryImage.DisplayOrder = 1;
                    }
                    else
                    {
                        existingProduct.ProductImages.Add(
                            new ProductImage
                            {
                                ImageUrl = uploadResult.ImageUrl,
                                PublicId = uploadResult.PublicId,
                                AltText = existingProduct.Name,
                                IsPrimary = true,
                                DisplayOrder = 1,
                                CreatedAt = DateTime.UtcNow
                            });
                    }
                }

                await _productService.UpdateAsync(existingProduct);

                // Delete the old Cloudinary image only after DB save succeeds.
                if (!string.IsNullOrWhiteSpace(oldPublicId))
                {
                    try
                    {
                        await _imageService.DeleteAsync(oldPublicId);
                    }
                    catch (Exception deleteException)
                    {
                        _logger.LogError(
                            deleteException,
                            "Product updated, but old Cloudinary image could not be deleted.");
                    }
                }

                TempData["SuccessMessage"] =
                    "Product updated successfully.";

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Product update or image replacement failed.");

                // Remove the new Cloudinary image if DB save failed.
                if (!string.IsNullOrWhiteSpace(newlyUploadedPublicId))
                {
                    try
                    {
                        await _imageService.DeleteAsync(
                            newlyUploadedPublicId);
                    }
                    catch (Exception cleanupException)
                    {
                        _logger.LogError(
                            cleanupException,
                            "Failed to remove the newly uploaded Cloudinary image.");
                    }
                }

                ModelState.AddModelError(
                    string.Empty,
                    $"The product could not be updated: {ex.Message}");

                await LoadCategoriesAsync();

                SetCurrentImage(existingProduct);

                return Page();
            }
        }

        private void ValidateImage()
        {
            // Image is optional during Edit.
            // If no new image is selected, keep the current image.
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

            var extension = Path
                .GetExtension(ImageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            const long maximumFileSize =
                10 * 1024 * 1024;

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

            if (string.IsNullOrWhiteSpace(
                    ImageFile.ContentType) ||
                !allowedContentTypes.Contains(
                    ImageFile.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Please select a valid JPG, PNG or WEBP image.");
            }
        }

        private async Task LoadCurrentImageAsync()
        {
            var existingProduct =
                await _productService.GetByIdAsync(Product.Id);

            if (existingProduct != null)
            {
                SetCurrentImage(existingProduct);
            }
        }

        private void SetCurrentImage(Product product)
        {
            CurrentImageUrl =
                product.ProductImages?
                    .FirstOrDefault(image => image.IsPrimary)
                    ?.ImageUrl
                ?? product.ProductImages?
                    .FirstOrDefault()
                    ?.ImageUrl;
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