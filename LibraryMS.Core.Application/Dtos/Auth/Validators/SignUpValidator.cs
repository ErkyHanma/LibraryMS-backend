using FluentValidation;

namespace LibraryMS.Core.Application.Dtos.Auth.Validators
{
    public class SignUpValidator : AbstractValidator<SignUpDto>
    {
        public SignUpValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50)
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Name can only contain letters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50)
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Last name can only contain letters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100);


            RuleFor(x => x.UniversityId)
                .NotEmpty()
                .Length(5, 20)
                .WithMessage("University ID format is invalid.");
        }
    }
}