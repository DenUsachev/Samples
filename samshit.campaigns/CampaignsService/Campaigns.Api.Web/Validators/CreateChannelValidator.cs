using System;
using Campaigns.Api.Web.Domain;
using FluentValidation;

namespace Campaigns.Api.Web.Validators
{
    public class CreateChannelValidator : AbstractValidator<CreateChannelRequest>
    {
        public CreateChannelValidator()
        {
            var minChannelTypeNum = 0;
            var maxChannelTypeNum = Enum.GetValues(typeof(ChannelType)).Length;
            RuleFor(x => x.CampaignId).GreaterThan(0);
            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.MetaJson).NotNull().NotEmpty();
            RuleFor(x => x.ChannelType).Transform(type => (int) type).GreaterThan(minChannelTypeNum).LessThan(maxChannelTypeNum);
        }
    }
}