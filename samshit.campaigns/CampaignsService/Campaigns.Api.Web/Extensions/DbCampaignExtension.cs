using System;
using System.Linq;
using Campaigns.Api.Web.Domain;
using Samshit.DbAccess.Postgre.Campaigns;

namespace Campaigns.Api.Web.Extensions
{
    public static class DbCampaignExtension
    {
        public static Campaign ToDomain(this DbCampaign entity, bool hierarchical = false)
        {
            var campaign = new Campaign
            {
                Id = entity.Id,
                Name = entity.Name,
                OwnerId = entity.OwnerId,
                Created = entity.Created,
                Updated = entity.Updated
            };
            if (entity.CommChannels != null && hierarchical)
            {
                campaign.Channels = entity.CommChannels.Select(it => it.Id).ToArray();
            }

            return campaign;
        }

        public static DbCampaign ToEntity(this Campaign domain)
        {
            var dbCampaign = new DbCampaign
            {
                Id = domain.Id,
                Name = domain.Name,
                //todo fix!!!
                OwnerId = (int) domain.OwnerId,
                Created = domain.Created,
                Updated = domain.Updated.GetValueOrDefault(DateTime.Now),
            };
            return dbCampaign;
        }
    }
}