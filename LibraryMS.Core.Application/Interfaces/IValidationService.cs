namespace LibraryMS.Core.Application.Interfaces
{
    public interface IValidationService
    {
        Task ValidateAsync<T>(T instance) where T : class;
    }
}
