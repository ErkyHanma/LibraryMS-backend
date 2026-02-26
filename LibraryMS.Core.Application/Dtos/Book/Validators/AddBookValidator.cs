using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace LibraryMS.Core.Application.Dtos.Book.Validators
{
    public class AddBookValidator : AbstractValidator<AddBookDto>
    {
        public AddBookValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.")
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(1000);

            RuleFor(x => x.Summary)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(2000);

            RuleFor(x => x.CategoryIds)
                .NotNull()
                .NotEmpty().WithMessage("At least one category is required.")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("Duplicate category IDs are not allowed.");

            RuleFor(x => x.Pages)
                .GreaterThan(0)
                .WithMessage("Pages must be greater than zero.");

            RuleFor(x => x.PublishDate)
                 .NotNull()
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Publish date cannot be in the future.");

            RuleFor(x => x.TotalCopies)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.AvailableCopies)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(x => x.TotalCopies)
                .WithMessage("Available copies cannot exceed total copies.");

            // Cover image validation
            RuleFor(x => x.CoverFile)
                .NotNull().WithMessage("Cover image is required.")
                .Must(BeValidImage)
                .WithMessage("Cover image must be a valid image file (jpg, jpeg, png, webp) and under 5MB.");
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null) return false;

            var allowedTypes = new[]
            {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

            return allowedTypes.Contains(file.ContentType)
                   && file.Length > 0
                   && file.Length <= 5 * 1024 * 1024;
        }
    }
}
