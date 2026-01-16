using LibraryMS_API.Core.Application.Dtos.Image;
using Microsoft.AspNetCore.Http;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<bool> DeleteImageAsync(string publicId);
        Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder);
    }
}