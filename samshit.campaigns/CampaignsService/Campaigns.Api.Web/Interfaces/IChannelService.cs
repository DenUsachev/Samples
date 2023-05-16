using System.Collections.Generic;
using System.Threading.Tasks;
using Campaigns.Api.Web.Domain;

namespace Campaigns.Api.Web.Interfaces
{
    public interface IChannelService
    {
        Task<Channel> Create(CreateChannelRequest createRequest);
        Task<Channel> Update(Channel channel);
        Task<bool> Delete(int channelId);
        Task<IEnumerable<Channel>> Get(long userId, int[] campaignIds);
        Task<Channel> Get(int id);
        Task<Channel> GetByToken(string token);
    }
}