using System;

namespace Campaigns.Api.Web.Domain
{
    public abstract class ChannelMetaBase
    {
        public abstract string Token { get; set; }
        public abstract Int32 CampaignId { get; set; }
    }
}