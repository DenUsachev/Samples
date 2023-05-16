using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;

namespace Samshit.AuthGateway.Services
{
    public class AuthGrpcBridge : AuthGrpcService.AuthGrpcServiceBase
    {
        private readonly ILogger<AuthGrpcBridge> _logger;
        private readonly IUserService _userService;

        public AuthGrpcBridge(ILogger<AuthGrpcBridge> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override async Task<SessionResponse> CreateSession(SessionRequest request, ServerCallContext context)
        {
            try
            {
                var session = await _userService.CreateWidgetSessionToken(new SessionRequestDto
                {
                    CampaignId = request.CampaignId,
                    UserId = request.UserId
                });
                return new SessionResponse {IsSuccess = true, SessionJwtToken = session.jwtToken, SessionId = session.Id};
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
        }

        public override async Task<BearerResponse> VerifyBearer(BearerRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"New bearer verificartion request for: {request.Bearer}");
                var userDto = await _userService.GetUserByBearer(request.Bearer);
                var userFull = await _userService.Get(userDto.Login);
                return new BearerResponse
                {
                    IsSuccess = userDto != null,
                    UserId = userDto?.UserId ?? 0,
                    SessionId = userFull.RefreshToken
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new RpcException(Status.DefaultCancelled, e.Message);
            }
        }
    }
}