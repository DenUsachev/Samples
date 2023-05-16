using FluentValidation;
using Samshit.DataModels.UserDomain;

namespace Samshit.Validation.UserDomain
{
    public class UserModelValidator : AbstractValidator<UserModelDto>
    {
        public UserModelValidator()
        {
            RuleFor(it => it.Login).EmailAddress();
            RuleFor(it => it.LinkedAccounts).Must(it => it != null && it.Length > 0);
            RuleForEach(it => it.LinkedAccounts).Must(it => it.FirstName != null && it.LastName != null && it.Email != null);
        }
    }
}
