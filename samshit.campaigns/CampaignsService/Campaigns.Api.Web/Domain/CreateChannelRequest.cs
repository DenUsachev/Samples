using Newtonsoft.Json;

namespace Campaigns.Api.Web.Domain
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
    public class CreateChannelRequest
    {
        public int CampaignId { get; set; }
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
        public bool IsActive { get; set; }
        public string MetaJson { get; set; } 
        
        public CreateChannelRequest()
        {
            IsActive = true;
        }
    }
}