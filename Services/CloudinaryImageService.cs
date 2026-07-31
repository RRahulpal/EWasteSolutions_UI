using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EWasteSolutions.Models;
using Microsoft.Extensions.Options;

namespace EWasteSolutions.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageService(
            IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string ImageUrl, string PublicId)> UploadAsync(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Image file is required.");
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ewaste-products",
                Transformation = new Transformation()
                    .Width(1000)
                    .Height(1000)
                    .Crop("limit")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new InvalidOperationException(result.Error.Message);
            }

            return (
                result.SecureUrl.ToString(),
                result.PublicId
            );
        }

        public async Task DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var deleteParams = new DeletionParams(publicId);

            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}