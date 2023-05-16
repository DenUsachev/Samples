using Campaigns.Api.Web.Domain;
using Samshit.DbAccess.Postgre.Campaigns;

namespace Campaigns.Api.Web.Extensions
{
    public static class ChannelAttributeExtensions
    {
        public static ChannelAttributeModel ToDomain(this ChannelAttribute entity)
        {
            var model = new ChannelAttributeModel
            {
                Value = entity.Value,
                Name = entity.Name,
                Type = entity.Type,
                ChannelId = entity.ChannelId,
                Id = entity.Id
            };
            return model;
        }

        public static ChannelAttribute ToEntity(this ChannelAttributeModel model)
        {
            var entity = new ChannelAttribute
            {
                Value = model.Value,
                Name = model.Name,
                Type = model.Type,
                ChannelId = model.ChannelId,
                Id = model.Id, 
            };
            return entity;
        }
    }
}
