using Application.Features.Users.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Validators
{
    public class UserRegistrationCommandValidator : AbstractValidator<UserRegistrationCommand>
    {
        public UserRegistrationCommandValidator()
        {
            ////One way to validate nested properties
            //RuleFor(x => x.UserRegistration).NotNull().WithMessage("User registration details are required.");
            //When(x => x.UserRegistration != null, () =>
            //{
            //    RuleFor(x => x.UserRegistration.Email)
            //        .NotEmpty().WithMessage("Email is required.")
            //        .EmailAddress().WithMessage("A valid email is required.");
            //    RuleFor(x => x.UserRegistration.FirstName)
            //        .NotEmpty().WithMessage("First name is required.");
            //});

            // or you can do this way by reusing the existing validator
            RuleFor(x => x.UserRegistration)
                .SetValidator(new UserRegistrationRequestValidator());
        }
    }
}
