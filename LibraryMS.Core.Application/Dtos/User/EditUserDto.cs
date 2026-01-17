using Microsoft.AspNetCore.Http;

namespace LibraryMS.Core.Application.Dtos.User
{
    public class EditUserDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
    }
}
