using Campaigns.Api.Web.Domain;
using FluentValidation;

namespace Campaigns.Api.Web.Validators
{
    public class CreateCampaignValidator : AbstractValidator<CreateCampaignRequest>
    {
        public CreateCampaignValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }
}