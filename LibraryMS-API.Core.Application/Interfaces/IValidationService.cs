namespace LibraryMS_API.Core.Application.Interfaces
{
    public interface IValidationService
    {
        Task ValidateAsync<T>(T instance) where T : class;
    }
}
