using LibraryMS_API.Core.Application.Dtos.Email;

namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}
