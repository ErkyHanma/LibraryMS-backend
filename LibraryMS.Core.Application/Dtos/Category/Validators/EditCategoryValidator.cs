using FluentValidation;

namespace LibraryMS.Core.Application.Dtos.Category.Validators
{
    public class EditCategoryValidator : AbstractValidator<EditCategoryDto>
    {
        public EditCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.UpdatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("UpdatedAt cannot be in the future.");
        }
    }
}