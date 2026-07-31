namespace EWasteSolutions.Services
{
    public interface IImageService
    {
        Task<(string ImageUrl, string PublicId)> UploadAsync(IFormFile file);

        Task DeleteAsync(string publicId);
    }
}
