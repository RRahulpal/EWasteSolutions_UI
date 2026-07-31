using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EWasteSolutions.Models;
using Microsoft.Extensions.Options;

namespace EWasteSolutions.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageService> _logger;

        public CloudinaryImageService(
            IOptions<CloudinarySettings> options,
            ILogger<CloudinaryImageService> logger)
        {
            _logger = logger;

            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.CloudName))
            {
                throw new InvalidOperationException(
                    "Cloudinary CloudName is missing.");
            }

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException(
                    "Cloudinary ApiKey is missing.");
            }

            if (string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary ApiSecret is missing.");
            }

            var account = new Account(
                settings.CloudName.Trim(),
                settings.ApiKey.Trim(),
                settings.ApiSecret.Trim());

            _cloudinary = new Cloudinary(account);

            _cloudinary.Api.Secure = true;
        }

        public async Task<(string ImageUrl, string PublicId)> UploadAsync(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "The image file is empty.",
                    nameof(file));
            }

            await using var stream = file.OpenReadStream();

            var uploadParameters = new ImageUploadParams
            {
                File = new FileDescription(
                    file.FileName,
                    stream),

                Folder = "ewaste-products",

                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,

                Transformation = new Transformation()
                    .Width(1200)
                    .Height(1200)
                    .Crop("limit")
                    .Quality("auto")
            };

            _logger.LogInformation(
                "Sending image {FileName} to Cloudinary.",
                file.FileName);

            var result =
                await _cloudinary.UploadAsync(uploadParameters);

            if (result.Error != null)
            {
                throw new InvalidOperationException(
                    $"Cloudinary error: {result.Error.Message}");
            }

            if (result.StatusCode is not
                System.Net.HttpStatusCode.OK and not
                System.Net.HttpStatusCode.Created)
            {
                throw new InvalidOperationException(
                    $"Cloudinary returned status code {result.StatusCode}.");
            }

            var imageUrl = result.SecureUrl?.ToString();

            if (string.IsNullOrWhiteSpace(imageUrl) ||
                string.IsNullOrWhiteSpace(result.PublicId))
            {
                throw new InvalidOperationException(
                    "Cloudinary upload completed without returning valid image details.");
            }

            _logger.LogInformation(
                "Cloudinary image uploaded successfully with public ID {PublicId}.",
                result.PublicId);

            return (imageUrl, result.PublicId);
        }

        public async Task DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var deleteParameters = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image,
                Invalidate = true
            };

            var result =
                await _cloudinary.DestroyAsync(deleteParameters);

            if (result.Error != null)
            {
                throw new InvalidOperationException(
                    $"Cloudinary deletion error: {result.Error.Message}");
            }
        }
    }
}