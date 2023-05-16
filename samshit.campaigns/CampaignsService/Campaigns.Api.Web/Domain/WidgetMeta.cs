namespace Campaigns.Api.Web.Domain
{
    public class WidgetMeta : ChannelMetaBase
    {
        public string SiteAddress { get; set; }
        public override string Token { get; set; }
        public override int CampaignId { get; set; }
    }
}
