using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Campaigns.Api.Web.Domain;
using Campaigns.Api.Web.Extensions;
using Campaigns.Api.Web.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Samshit.DbAccess.Postgre;
using Samshit.DbAccess.Postgre.Campaigns;
using Samshit.WebUtils;

namespace Campaigns.Api.Web.Services
{
    public class ChannelService : IChannelService
    {
        private readonly CampaignsContext _ctx;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public ChannelService(DbContext ctx)
        {
            _ctx = (CampaignsContext) ctx;
        }

        public async Task<Channel> Create(CreateChannelRequest createRequest)
        {
            var dbChannel = new DbCommChannel
            {
                Name = createRequest.Name,
                Type = (int) createRequest.ChannelType,
                CampaignId = createRequest.CampaignId,
                IsActive = createRequest.IsActive,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };

            // processing channel meta
            ChannelMetaBase meta = null;
            if (!string.IsNullOrEmpty(createRequest.MetaJson))
            {
                switch (createRequest.ChannelType)
                {
                    case ChannelType.Widget:
                        meta = JsonConvert.DeserializeObject<WidgetMeta>(createRequest.MetaJson);
                        meta.Token = PasswordHelper.GetDomainKey(((WidgetMeta) meta).SiteAddress);
                        meta.CampaignId = createRequest.CampaignId;
                        break;
                    default:
                        meta = null;
                        break;
                }
            }

            if (meta == null)
            {
                return null;
            }

            using (var transaction = await _ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    await _ctx.CommChannels.AddAsync(dbChannel);
                    await _ctx.SaveChangesAsync();

                    Channel createdChannel = dbChannel.ToDomain();
                    var (isWritten, attributes) = PrepareMeta(dbChannel.Id, meta);
                    if (!isWritten)
                    {
                        throw new Exception("Failed to write changes to DB: Channel.");
                    }

                    foreach (var attribute in attributes)
                    {
                        var dbAttribute = attribute.ToEntity();
                        _ctx.Add(dbAttribute);
                    }

                    await _ctx.SaveChangesAsync();
                    await transaction.CommitAsync();
                    createdChannel.Attributes = attributes;
                    createdChannel.MetaJson = JsonConvert.SerializeObject(meta, _jsonSerializerSettings);
                    return createdChannel;
                }
                catch (Exception _)
                {
                    await transaction.RollbackAsync();
                }
            }

            return null;
        }

        public async Task<Channel> Update(Channel channel)
        {
            var dbChannel = await _ctx.CommChannels.SingleOrDefaultAsync(it => it.Id == channel.Id);
            if (dbChannel != null)
            {
                dbChannel.Updated = DateTime.Now;
                dbChannel.IsActive = channel.IsActive;
                dbChannel.Name = channel.Name;
                var updated = await _ctx.SaveChangesAsync();
                if (updated > 0)
                {
                    return dbChannel.ToDomain();
                }
            }

            return null;
        }

        public async Task<Channel> Get(int id)
        {
            var channel = await _ctx.CommChannels.SingleOrDefaultAsync(it => it.Id == id);
            if (channel == null)
            {
                return null;
            }

            var channelModel = channel.ToDomain();
            //todo: fix meta extraction!
            //ExtractMeta(channelModel);
            return channelModel;
        }

        public async Task<IEnumerable<Channel>> Get(long userId, int[] campaignIds)
        {
            var channelsQuery = _ctx.CommChannels
                .Include(it => it.Attributes)
                .Include(it => it.Campaign);

            IList<DbCommChannel> _dbChannels;
            if (campaignIds.Length > 0)
            {
                _dbChannels = await channelsQuery.Where(it => it.Campaign.OwnerId == userId
                                                              && campaignIds.Contains(it.CampaignId)).ToListAsync();
            }
            else
            {
                _dbChannels = await channelsQuery.Where(it => it.Campaign.OwnerId == userId).ToListAsync();
            }

            var channels = _dbChannels.Select(it => it.ToDomain()).ToArray();
            foreach (var channel in channels)
            {
                ExtractMeta(channel);
            }

            return channels;
        }

        public async Task<Channel> GetByToken(string token)
        {
            const string propertyName = "Token";
            var channel = await _ctx.ChannelAttributes.Include(it => it.Channel.Attributes)
                .Where(it => it.Name == propertyName && it.Value == token)
                .Select(it => it.Channel).Where(it => it.IsActive)
                .SingleOrDefaultAsync();

            return channel?.ToDomain();
        }

        public async Task<bool> Delete(int channelId)
        {
            var dbChannel = await _ctx.CommChannels.SingleOrDefaultAsync(it => it.Id == channelId);
            if (dbChannel != null)
            {
                _ctx.Remove(dbChannel);
                var deleted = await _ctx.SaveChangesAsync();
                if (deleted > 0)
                {
                    return true;
                }

                throw new Exception($"Failed to delete entity {nameof(DbCommChannel)}");
            }

            return false;
        }

        public (bool, ChannelAttributeModel[]) PrepareMeta(int channelId, ChannelMetaBase meta)
        {
            try
            {
                var props = meta.GetType()?.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var attributes = new ChannelAttributeModel[props.Length];
                var i = 0;
                foreach (var info in props)
                {
                    var propertyName = info.Name;
                    var value = info.GetValue(meta)?.ToString();
                    attributes[i++] = ChannelAttributeModel.Create(channelId, propertyName, value);
                }

                return (true, attributes);
            }
            catch (Exception _)
            {
                return (false, null);
            }
        }

        public void ExtractMeta(Channel channel)
        {
            ChannelMetaBase meta = null;
            switch (channel.ChannelType)
            {
                case ChannelType.Widget:
                    meta = new WidgetMeta();
                    break;
                default:
                    meta = null;
                    break;
            }

            if (meta == null)
            {
                throw new Exception("Unknown channel type. Unable to extract META.");
            }

            var props = meta.GetType()?.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var metaField in props)
            {
                var target = channel.Attributes.SingleOrDefault(it => it.Name == metaField.Name);
                if (target != null)
                {
                    if (ChannelAttributeModel.TryParse(target, out object extractedValue))
                    {
                        metaField.SetValue(meta, extractedValue);
                    }
                }
            }

            channel.MetaJson = JsonConvert.SerializeObject(meta, _jsonSerializerSettings);
        }
    }
}