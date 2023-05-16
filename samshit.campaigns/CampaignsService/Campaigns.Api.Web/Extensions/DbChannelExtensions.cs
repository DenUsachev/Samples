using System;
using System.Linq;
using Campaigns.Api.Web.Domain;
using Samshit.DbAccess.Postgre;

namespace Campaigns.Api.Web.Extensions
{
    public static class DbChannelExtensions
    {
        public static Channel ToDomain(this DbCommChannel entity)
        {
            var channel = new Channel
            {
                Id = entity.Id,
                CampaignId = entity.CampaignId,
                ChannelType = (ChannelType) entity.Type,
                Name = entity.Name,
                Created = entity.Created,
                Updated = entity.Updated,
                IsActive = entity.IsActive
            };

            if (entity.Attributes != null && entity.Attributes.Any())
            {
                channel.Attributes = entity.Attributes.Select(it => it.ToDomain()).ToArray();
            }
            return channel;
        }

        public static DbCommChannel ToEntity(this Channel domain)
        {
            return new DbCommChannel
            {
                Id = domain.Id,
                CampaignId = domain.CampaignId,
                Name = domain.Name,
                Created = domain.Created,
                Updated = domain.Updated.GetValueOrDefault(DateTime.Now),
                IsActive = domain.IsActive,
                Type = (int)domain.ChannelType
            };
        }
    }
}
