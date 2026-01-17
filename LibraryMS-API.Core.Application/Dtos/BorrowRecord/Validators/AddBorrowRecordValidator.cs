using FluentValidation;

namespace LibraryMS_API.Core.Application.Dtos.BorrowRecord.Validators
{
    public class AddBorrowRecordValidator : AbstractValidator<AddBorrowRecordDto>
    {
        public AddBorrowRecordValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.BookId)
                .GreaterThan(0)
                .WithMessage("BookId must be a valid ID.");

            RuleFor(x => x.BorrowDate)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Borrow date cannot be in the future.");
        }
    }
}
