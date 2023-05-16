using System;
using System.Linq;
using System.Threading.Tasks;
using Campaigns.Api.Web.Domain;
using Campaigns.Api.Web.Filters;
using Campaigns.Api.Web.Interfaces;
using Campaigns.Api.Web.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.WebUtils;

namespace Campaigns.Api.Web.Controllers
{
    [Produces("application/json")]
    [Route("campaigns/v1/channel")]
    public class ChannelsController : ApiControllerBase
    {
        private ILogger<ChannelsController> _logger;
        private IChannelService _service;

        public ChannelsController(ILogger<ChannelsController> logger, IChannelService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// Get channel by Id
        /// </summary>
        /// <param name="id">Channel Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Channel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Fetch(int id)
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var channels = (await _service.Get(userId, new[] { id })).ToArray();
                    if (channels.Length > 0)
                    {
                        return Ok(new OperationResult<Channel>()
                        {
                            Data = channels[0],
                            IsSuccess = true
                        });
                    }

                    return NotFound(new OperationResult<object>
                    {
                        IsSuccess = false,
                        ErrorCode = ErrorCodes.EntityNotFoundError,
                    });
                }
                catch (Exception e)
                {
                    var errorMessage = $"Failed to get Channel: {id}. {e.Message}";
                    _logger.LogError(errorMessage);
                    return BadRequest(new OperationResult<object>()
                    {
                        IsSuccess = false,
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage
                    });
                }
            }

            return Unauthorized(new OperationResult<object>()
            {
                IsSuccess = false,
                ErrorCode = ErrorCodes.LoginError,
            });
        }

        /// <summary>
        /// Communication Channels for specified User
        /// Can be filtered by CampaignId (see query params)
        /// </summary>
        /// <param name="ids">An array-style specified IDs of Campaigns. Ex: campaigns[0]=1111&campaigns[1]=333...</param>
        /// <returns></returns>
        [HttpGet]
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Channel[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetByIds([FromQuery(Name = "campaigns")] int[] ids)
        {
            try
            {
                if (TryPassSessionCheck(out _, out long userId))
                {
                    var channels = await _service.Get(userId, ids);
                    var result = channels?.ToArray() ?? Enumerable.Empty<Channel>();

                    return Ok(new OperationResult<Channel[]>
                    {
                        IsSuccess = true,
                        Data = result.ToArray()
                    });
                }

                return Unauthorized(new OperationResult<object>
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.LoginError,
                });
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to fetch Channels: {ids}. {e.Message}";
                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorData = errorMessage,
                    ErrorCode = ErrorCodes.GenericError
                });
            }
        }

        /// <summary>
        /// Creates new Communication Channel
        /// </summary>
        /// <param name="request">Create request</param>
        /// <returns></returns>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Channel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateChannelRequest request)
        {
            try
            {
                var validator = new CreateChannelValidator();
                var validatorResult = await validator.ValidateAsync(request);
                if (validatorResult.IsValid)
                {
                    var channel = await _service.Create(request);
                    if (channel == null)
                    {
                        const string error = "Failed to create Channel. DB constraint violation?";
                        _logger.LogError(error);
                        return BadRequest(new ErrorData(ErrorCodes.ModelAddError, new Guid(), error));
                    }

                    return Ok(new OperationResult<Channel>()
                    {
                        IsSuccess = true,
                        Data = channel
                    });
                }

                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.ModelError
                });
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to create Channel:{e.Message}";
                _logger.LogError(errorMessage, e);
                return BadRequest(new OperationResult<object>
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.GenericError,
                    ErrorData = errorMessage
                });
            }
        }

        /// <summary>
        /// Updates Communication Channel
        /// </summary>
        /// <param name="channel">Channel object to update</param>
        /// <returns></returns>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Channel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpPatch]
        public async Task<ActionResult> Update(Channel channel)
        {
            try
            {
                var updated = await _service.Update(channel);
                if (updated != null)
                {
                    return Ok(new OperationResult<Channel>()
                    {
                        IsSuccess = true,
                        Data = updated
                    });
                }

                var errorMessage = $"Failed to update Channel {channel.Id}";
                _logger.LogError(errorMessage);
                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.ModelEditError,
                    ErrorData = errorMessage
                });
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to update Channel {channel.Id}: {e.Message}";
                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.GenericError,
                    ErrorData = errorMessage
                });
            }
        }

        /// <summary>
        /// Deletes Communication Channel
        /// </summary>
        /// <param name="id">Id of the Channel to be deleted</param>
        /// <returns></returns>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                if (deleted)
                {
                    return Ok(new OperationResult<object>()
                    {
                        IsSuccess = true
                    });
                }

                var errorMessage = $"Failed to delete Channel {id}";
                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.ModelEditError,
                    ErrorData = errorMessage
                });
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to delete Channel {id}:{e.Message}";
                return BadRequest(new OperationResult<object>()
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.GenericError,
                    ErrorData = errorMessage
                });
            }
        }
    }
}