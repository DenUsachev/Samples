using FluentValidation;
using Samshit.DataModels.UserDomain;

namespace Samshit.Validation.UserDomain
{
    public class ResetPasswordValidator : AbstractValidator<UserModelUpdatePasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(it => it.RecoveryToken).NotEmpty().NotNull();
            RuleFor(it => it.Password).NotEmpty().NotNull();
        }
    }
}
