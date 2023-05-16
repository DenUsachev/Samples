using System.Collections.Generic;
using System.Threading.Tasks;
using Campaigns.Api.Web.Domain;

namespace Campaigns.Api.Web.Interfaces
{
    public interface ICampaignsService
    {
        Task<List<Campaign>> List(long userId);
        Task<Campaign> Get(int id, long userId);
        Task<Campaign> Create(Campaign campaign, long userId);
        Task<Campaign> Update(Campaign campaign, long userId);
        Task<bool> Delete(int id, long userId);
    }
}