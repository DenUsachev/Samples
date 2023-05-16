using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.FIlters;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;
using Samshit.WebUtils;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("auth/v1/session")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private readonly SamshitDbContext _ctx;
        private readonly IUserService _service;

        public SessionController(ILogger<SessionController> logger, DbContext ctx, IUserService service)
        {
            _logger = logger;
            _ctx = (SamshitDbContext) ctx;
            _service = service;
        }

        /// <summary>
        /// Initializes login sequence
        /// </summary>
        /// <param name="login">User name to be logged in</param>
        /// <returns>New session token or error</returns>
        /// <response code="200">Returns the newly created token</response>
        /// <response code="400">Internal error during acquiring token</response>
        [HttpGet]
        [Route("{login}")]
        [ProducesResponseType(typeof(OperationResult<object>), 200)]
        [ProducesResponseType(typeof(OperationResult<object>), 400)]
        public async Task<ActionResult> GetToken([FromRoute] string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.LoginError));
            }

            try
            {
                var tokenValue = await _service.GetToken(login);
                if (tokenValue != null)
                {
                    return Ok(OperationResult<object>.CreateOk(new {Token = tokenValue}));
                }

                return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.LoginError));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.LoginError));
            }
        }

        /// <summary>
        /// Finalizes login sequence and returns User object and its session tokens
        /// </summary>
        /// <param name="model">Login model. Fields token, password</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), 200)]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), 403)]
        public async Task<ActionResult> Login([FromBody] LoginModelDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.ModelError));
                }

                var authorizedUser = await _service.Login(model.Token, model.Password);
                if (authorizedUser != null)
                {
                    var response = new LoginResponse
                    {
                        RefreshToken = authorizedUser.RefreshToken,
                        AccessToken = authorizedUser.AccessToken
                    };

                    return Ok(OperationResult<LoginResponse>.CreateOk(response));
                }

                return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.LoginError));
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(Login)} method call failed: {e.Message}");
                return BadRequest(OperationResult<object>.CreateFail(ErrorCodes.LoginError, e.Message));
            }
        }

        /// <summary>
        /// Issues the new access/refresh tokens pair by provided refresh token.
        /// </summary>
        /// <param name="request">exist refresh token</param>
        /// <returns>An object with new pair of tokens or error</returns>
        /// <response code="200">The tokens was successfully updated. A new access tokens with updated claims got</response>
        /// <response code="401">Incorrect token or token out of date. Session terminated. New session required.</response>
        /// <response code="400">Model error. See error code.</response>
        [HttpPatch]
        [BearerCheckFilter]
        [ProducesResponseType(typeof(OperationResult<RefreshTokenResult>), 200)]
        [ProducesResponseType(typeof(OperationResult<RefreshTokenResult>), 400)]
        [ProducesResponseType(typeof(UnauthorizedResult), 401)]
        public async Task<ActionResult> GetRefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(OperationResult<RefreshTokenResult>.CreateFail(ErrorCodes.ModelError));
            }

            try
            {
                var tokenOwner = await _service.GetUserByRefreshToken(request.Token);
                if (tokenOwner != null)
                {
                    var tokenResult = await _service.RefreshToken(tokenOwner.Login, request.Token);
                    if (tokenResult != null)
                    {
                        return Ok(OperationResult<RefreshTokenResult>.CreateOk(tokenResult));
                    }
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GetRefreshToken)} method call failed: {e.Message}");
                return BadRequest(OperationResult<RefreshTokenResult>.CreateFail(ErrorCodes.LoginError));
            }
        }

        /// <summary>
        /// Finalizes current user session
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Session successfully terminated</response>
        /// <response code="400">Unrecognized error. You should terminate all user sessions' data and redirect him to home.</response>
        [HttpDelete]
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult), 200)]
        [ProducesResponseType(typeof(OperationResult), 400)]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var currentPrincipal = this.User;
                if (currentPrincipal?.Identity != null)
                {
                    var login = ClaimsHelper.GetPrincipalClaimsByName(currentPrincipal, ClaimNames.NAME)
                        .FirstOrDefault();
                    if (login != null)
                    {
                        var logoutResult = await _service.Logout(login.Value);
                        if (logoutResult)
                        {
                            return Ok(new OperationResult {IsSuccess = true});
                        }
                    }
                }

                return BadRequest(new OperationResult
                {
                    IsSuccess = false, ErrorData = "Could not logout user session", ErrorCode = ErrorCodes.LogoutError
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, this.User);
                return BadRequest(new OperationResult
                    {IsSuccess = false, ErrorData = e.Message, ErrorCode = ErrorCodes.LogoutError});
            }
        }
    }
}