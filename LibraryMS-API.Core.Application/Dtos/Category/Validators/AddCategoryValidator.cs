using FluentValidation;

namespace LibraryMS_API.Core.Application.Dtos.Category.Validators
{
    public class AddCategoryValidator : AbstractValidator<AddCategoryDto>
    {
        public AddCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(10).WithMessage("Category name must not exceed 100 characters.");

        }
    }
}
