using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LibraryMS.Core.Application.Dtos.Image;
using LibraryMS.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace LibraryMS.Infrastructure.Shared.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(acc) { Api = { Secure = true } };
        }

        public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder)
        {
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, and WebP are allowed.");

            // Validate file size (e.g., max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit.");

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation()
                              .Width(1000)
                              .Height(1000)
                              .Crop("limit")
                              .Quality("auto"),
                UniqueFilename = true,
                Overwrite = false

            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");


            return new ImageUploadResultDto()
            {
                FileImageKey = uploadResult.PublicId,
                FileImageUrl = uploadResult.SecureUrl.ToString(),
            };
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }

    }
}
