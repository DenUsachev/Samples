using System;
using System.Linq;
using Newtonsoft.Json;

namespace Campaigns.Api.Web.Domain
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long OwnerId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public int[] Channels { get; set; }

        public Campaign()
        {
            Created = DateTime.Now;
            Updated = DateTime.Now;
            Channels = Enumerable.Empty<int>().ToArray();
        }
    }
}