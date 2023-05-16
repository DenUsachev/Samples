using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Campaigns.Api.Web.Domain;
using Campaigns.Api.Web.Extensions;
using Campaigns.Api.Web.Interfaces;
using Microsoft.EntityFrameworkCore;
using Samshit.DbAccess.Postgre.Campaigns;

namespace Campaigns.Api.Web.Services
{
    public class CampaignsService : ICampaignsService
    {
        private readonly CampaignsContext _ctx;

        public CampaignsService(DbContext context)
        {
            _ctx = (CampaignsContext) context;
        }


        public async Task<Campaign> Create(Campaign campaign, long userId)
        {
            var entity = campaign.ToEntity();
            await _ctx.AddAsync(entity);
            int changes = await _ctx.SaveChangesAsync();
            if (changes > 0)
            {
                var campaignId = entity.Id;
                var binding = new CampaignUsers
                {
                    CampaignId = campaignId,
                    UserId = (int) userId,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };
                await _ctx.CampaignUsers.AddAsync(binding);
                await _ctx.SaveChangesAsync();
                return entity.ToDomain();
            }

            return null;
        }

        public async Task<List<Campaign>> List(long userId)
        {
            var campaignIds = await _ctx.CampaignUsers
                .Where(it => it.UserId == userId)
                .Select(it => it.CampaignId)
                .ToArrayAsync();

            var campaigns = await _ctx.Campaigns
                .Include(it => it.CommChannels)
                .Where(it => campaignIds.Contains(it.Id)).ToListAsync();
            if (campaigns.Any())
            {
                return campaigns.Select(it => it.ToDomain(hierarchical: true)).ToList();
            }

            return Enumerable.Empty<Campaign>().ToList();
        }

        public async Task<Campaign> Get(int id, long userId)
        {
            var campaignIds = await _ctx.CampaignUsers
                .Where(it => it.UserId == userId)
                .Select(it => it.CampaignId)
                .ToArrayAsync();

            var dbCampaign = await _ctx.Campaigns
                .Include(it => it.CommChannels)
                .SingleOrDefaultAsync(it => it.Id == id && campaignIds.Contains(it.Id));
            return dbCampaign?.ToDomain(hierarchical: true);
        }

        public async Task<Campaign> Update(Campaign campaign, long userId)
        {
            var campaignIds = await _ctx.CampaignUsers
                .Where(it => it.UserId == userId)
                .Select(it => it.CampaignId)
                .ToArrayAsync();

            var dbCampaign =
                await _ctx.Campaigns.SingleOrDefaultAsync(it => it.Id == campaign.Id && campaignIds.Contains(it.Id));
            if (dbCampaign != null)
            {
                dbCampaign.Name = campaign.Name;
                dbCampaign.Updated = DateTime.Now;
                await _ctx.SaveChangesAsync();
                return dbCampaign.ToDomain();
            }

            return null;
        }


        public async Task<bool> Delete(int id, long userId)
        {
            var campaignIds = await _ctx.CampaignUsers
                .Where(it => it.UserId == userId)
                .Select(it => it.CampaignId)
                .ToArrayAsync();

            var dbCampaign =
                await _ctx.Campaigns.SingleOrDefaultAsync(it => it.Id == id && campaignIds.Contains(it.Id));
            if (dbCampaign != null)
            {
                _ctx.Remove(dbCampaign);
                await _ctx.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}