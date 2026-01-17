using LibraryMS.Core.Application.Dtos.Image;
using Microsoft.AspNetCore.Http;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<bool> DeleteImageAsync(string publicId);
        Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder);
    }
}