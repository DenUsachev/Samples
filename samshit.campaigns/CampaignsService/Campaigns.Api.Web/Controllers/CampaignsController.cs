using System;
using System.Collections;
using System.Collections.Generic;
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
    [ApiController]
    [Produces("application/json")]
    [Route("campaigns/v1/campaigns")]
    public class CampaignsController : ApiControllerBase
    {
        private readonly ILogger<CampaignsController> _logger;
        private readonly ICampaignsService _campaignsService;

        public CampaignsController(ILogger<CampaignsController> logger, ICampaignsService service)
        {
            _logger = logger;
            _campaignsService = service;
        }

        /// <summary>
        /// Returns the list of the Campaigns available for this user
        /// </summary>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<IList<Campaign>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var campaigns = await _campaignsService.List(userId);
                    var result = new OperationResult<IList<Campaign>>
                    {
                        Data = campaigns,
                        ErrorCode = null,
                        IsSuccess = true
                    };
                    return Ok(result);
                }
                catch (Exception e)
                {
                    var errorMessage = "Failed to get Campaigns list.";
                    _logger.LogError(errorMessage, e);
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
            }

            return Unauthorized(new OperationResult<object>
            {
                ErrorCode = ErrorCodes.LoginError,
                ErrorData = null,
                IsSuccess = false
            });
        }

        /// <summary>
        /// Returns the Campaign by its ID.
        /// </summary>
        /// <param name="id">Campaign ID</param>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Campaign>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult> Fetch(int id)
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var campaign = await _campaignsService.Get(id, userId);
                    if (campaign == null)
                    {
                        return NotFound(new OperationResult<object>
                        {
                            ErrorCode = ErrorCodes.EntityNotFoundError,
                            ErrorData = null,
                            IsSuccess = false
                        });
                    }

                    return Ok(new OperationResult<Campaign>
                    {
                        IsSuccess = true,
                        Data = campaign,
                        ErrorData = null
                    });
                }
                catch (Exception e)
                {
                    var errorMessage = $"Failed to get Campaign {id} list.";
                    _logger.LogError(errorMessage, e);
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
            }

            return Unauthorized(new OperationResult<object>
            {
                ErrorCode = ErrorCodes.LoginError,
                ErrorData = null,
                IsSuccess = false
            });
        }

        /// <summary>
        /// Updates the campaign
        /// </summary>
        /// <param name="campaign">Campaign being updated</param>
        /// <returns>Result of the operation ot error if it was failed</returns>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Campaign>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpPatch]
        public async Task<ActionResult> Update(Campaign campaign)
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var updated = await _campaignsService.Update(campaign, userId);
                    if (updated != null)
                    {
                        return Ok(new OperationResult<Campaign>
                        {
                            IsSuccess = true,
                            Data = campaign,
                            ErrorData = null
                        });
                    }

                    var errorMessage = $"Failed to update Campaign {campaign.Id}";
                    _logger.LogError(errorMessage);
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.ModelError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
                catch (Exception e)
                {
                    var errorMessage = $"Failed to update Campaign {campaign.Id}: {e.Message}";
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
            }

            return Unauthorized(new OperationResult<object>
            {
                ErrorCode = ErrorCodes.LoginError,
                ErrorData = null,
                IsSuccess = false
            });
        }

        /// <summary>
        /// Deletes the campaign
        /// </summary>
        /// <param name="id">Id of the Campaign to be deleted</param>
        /// <returns>Result of the operation ot error if it was failed</returns>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var deleted = await _campaignsService.Delete(id, userId);
                    if (deleted)
                    {
                        return Ok(new OperationResult<object>
                        {
                            IsSuccess = true,
                        });
                    }

                    var errorMessage = $"Failed to delete Campaign: {id}";
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.ModelError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
                catch (Exception e)
                {
                    var errorMessage = $"Failed to delete Campaign: {e.Message}";
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
            }

            return Unauthorized(new OperationResult<object>
            {
                ErrorCode = ErrorCodes.LoginError,
                ErrorData = null,
                IsSuccess = false
            });
        }

        /// <summary>
        /// Creates the new Campaign and if it's succeeded, returns it's Id 
        /// </summary>
        /// <param name="request">Campaign creation request</param>
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<Campaign>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(OperationResult<object>), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateCampaignRequest request)
        {
            if (TryPassSessionCheck(out _, out long userId))
            {
                try
                {
                    var validator = new CreateCampaignValidator();
                    var validationResult = await validator.ValidateAsync(request);
                    if (validationResult.IsValid)
                    {
                        var campaign = new Campaign { Name = request.Name, OwnerId = userId };
                        var created = await _campaignsService.Create(campaign, userId);
                        if (created != null)
                            return Ok(new OperationResult<Campaign>
                            {
                                Data = created,
                                IsSuccess = true
                            });
                        var errorMessage = "Failed to create Campaign: Database insert operation error";
                        return BadRequest(new OperationResult<object>
                        {
                            ErrorCode = ErrorCodes.ModelAddError,
                            ErrorData = errorMessage,
                            IsSuccess = false
                        });
                    }

                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.ModelError,
                        ErrorData = validationResult.Errors,
                        IsSuccess = false
                    });
                }
                catch (Exception e)
                {
                    var errorMessage = $"Failed to create Campaign: Database insert operation error: {e.Message}";
                    return BadRequest(new OperationResult<object>
                    {
                        ErrorCode = ErrorCodes.GenericError,
                        ErrorData = errorMessage,
                        IsSuccess = false
                    });
                }
            }

            return Unauthorized(new OperationResult<object>
            {
                ErrorCode = ErrorCodes.LoginError,
                ErrorData = null,
                IsSuccess = false
            });
        }
    }
}