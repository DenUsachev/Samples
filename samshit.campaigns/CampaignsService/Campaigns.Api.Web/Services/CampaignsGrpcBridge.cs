using System;
using System.Linq;
using System.Threading.Tasks;
using Campaigns.Api.Web.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Campaigns.Api.Web.Services
{
    public class CampaignsGrpcBridge : ChannelGrpcService.ChannelGrpcServiceBase
    {
        private readonly ILogger<CampaignsGrpcBridge> _logger;
        private readonly IChannelService _channelService;
        private readonly ICampaignsService _campaignsService;

        public CampaignsGrpcBridge(ILogger<CampaignsGrpcBridge> logger, IChannelService channelService, ICampaignsService campaignsService)
        {
            _logger = logger;
            _channelService = channelService;
            _campaignsService = campaignsService;
        }

        public override async Task<CampaignUserResponse> GetCampaignsForUser(CampaignUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("New gRPC request handled: GetCampaignsForUser");
            try
            {
                var userCampaigns = await _campaignsService.List(request.UserId);
                var campaignsIdList = userCampaigns?.Select(it => it.Id).ToList();
                var response = new CampaignUserResponse();
                response.CampaignId.AddRange(campaignsIdList);
                response.IsSuccess = true;
                response.UserId = request.UserId;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
        }

        public override async Task<CampaignResponse> GetCampaignForChannel(CampaignRequest request, ServerCallContext context)
        {
            _logger.LogInformation("New gRPC request handled: GetCampaignForChannel");
            string error = default;
            try
            {
                if (request.ChannelId > 0)
                {
                    var channel = await _channelService.Get(request.ChannelId);
                    if (channel != null)
                    {
                        return new CampaignResponse {IsSuccess = true, CampaignId = channel.CampaignId};
                    }

                    _logger.LogError($"Channel with id={request.ChannelId} was not found");
                    return new CampaignResponse {IsSuccess = false, CampaignId = default};
                }

                throw new RpcException(Status.DefaultCancelled, "Invalid channel id");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
        }

        public override async Task<ChannelResponse> GetChannelForToken(ChannelRequest request, ServerCallContext context)
        {
            _logger.LogInformation("New gRPC request handled: GetChannelForToken");
            string error = default;
            try
            {
                if (!string.IsNullOrEmpty(request.Token))
                {
                    var commChannel = await _channelService.GetByToken(request.Token);
                    if (commChannel == null)
                    {
                        error = $"Channel for Token {request.Token} not found.";
                        _logger.LogError(error);
                        context.Status = new Status(StatusCode.NotFound, error);
                        return new ChannelResponse() {IsSuccess = false};
                    }

                    var channelProps = commChannel.Attributes.Select(it => new ChannelProperty()
                    {
                        Name = it.Name,
                        Value = it.Value
                    }).AsEnumerable();


                    var response = new ChannelResponse();
                    response.Prop.AddRange(channelProps);
                    response.IsSuccess = true;
                    context.Status = Status.DefaultSuccess;
                    return response;
                }

                error = "Failed to handle gRPC Request: token is null";
                _logger.LogError(error);
                context.Status = new Status(StatusCode.Unavailable, error);
                return new ChannelResponse {IsSuccess = false};
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
        }
    }
}