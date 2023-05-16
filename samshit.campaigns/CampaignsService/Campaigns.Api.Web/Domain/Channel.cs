using System;
using Newtonsoft.Json;

namespace Campaigns.Api.Web.Domain
{
    public class Channel
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ChannelType ChannelType { get; set; }
        public string MetaJson { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        [JsonIgnore]
        public ChannelAttributeModel[] Attributes { get; set; }

        public Channel()
        {
            Created = DateTime.Now;
            Updated = DateTime.Now;
        }
    }
}
