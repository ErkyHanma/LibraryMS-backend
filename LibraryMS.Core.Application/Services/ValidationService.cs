using FluentValidation;
using LibraryMS.Core.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryMS.Core.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ValidateAsync<T>(T instance) where T : class
        {
            var validators = _serviceProvider.GetServices<IValidator<T>>();

            if (!validators.Any())
                return; // No validators registered for this type

            var validationContext = new ValidationContext<T>(instance);

            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(validationContext)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                throw new Exceptions.ValidationException(failures);
            }
        }
    }
}