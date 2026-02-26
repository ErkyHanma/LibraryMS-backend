using FluentValidation;
using LibraryMS.Core.Application.Dtos.Book;
using Microsoft.AspNetCore.Http;

public class EditBookValidator : AbstractValidator<EditBookDto>
{
    public EditBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Author)
            .NotEmpty()
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
            .NotEmpty()
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate category IDs are not allowed.");

        RuleFor(x => x.Pages)
            .GreaterThan(0);

        RuleFor(x => x.PublishDate)
             .NotNull()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Publish date cannot be in the future.");

        RuleFor(x => x.TotalCopies)
            .GreaterThan(0);

        RuleFor(x => x.AvailableCopies)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.TotalCopies)
            .WithMessage("Available copies cannot exceed total copies.");

        RuleFor(x => x.UpdatedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.UpdatedAt.HasValue)
            .WithMessage("UpdatedAt cannot be in the future.");

        // Cover image is OPTIONAL for edit
        RuleFor(x => x.CoverFile)
            .Must(BeValidImage!)
            .When(x => x.CoverFile != null)
            .WithMessage("Cover image must be a valid image file (jpg, jpeg, png, webp) and under 5MB.");
    }

    private bool BeValidImage(IFormFile file)
    {
        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        return allowedTypes.Contains(file.ContentType)
               && file.Length > 0
               && file.Length <= 5 * 1024 * 1024;
    }
}
