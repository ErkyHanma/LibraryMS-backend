using LibraryMS.Core.Application.Dtos.Email;

namespace LibraryMS.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}
