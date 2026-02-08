using FluentValidation;
using ResponseResult.Models.Requests.Identity;

namespace Application.Features.Users.Validators
{
    public class UserRegistrationRequestValidator : AbstractValidator<UserRegistrationRequest>
    {
        public UserRegistrationRequestValidator()
        {
            RuleFor(x => x.Email)
                //.NotEmpty().WithMessage("Email is required.")
                .EmailAddress()/*.WithMessage("A valid email is required.")*/;
            RuleFor(x => x.FirstName)
                .NotEmpty();
        }
    }
}
