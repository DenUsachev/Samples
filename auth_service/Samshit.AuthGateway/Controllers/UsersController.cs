using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.Extensions;
using Samshit.AuthGateway.FIlters;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;
using Samshit.Validation.UserDomain;
using Samshit.WebUtils;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("auth/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly SamshitDbContext _ctx;
        private readonly IUserService _service;

        public UsersController(
            DbContext ctx,
            IUserService svc,
            ILogger<UsersController> logger)
        {
            _logger = logger;
            _service = svc;
            _ctx = (SamshitDbContext)ctx;
        }

        /// <summary>
        /// Returns current user profile
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Profile of the logged in user</response>
        /// <response code="401">Could not establish user identity. The session token might be invalid.</response>
        [HttpGet("me")]
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<UserModelSecuredDto>), 200)]
        [ProducesResponseType(typeof(UnauthorizedResult), 401)]
        public async Task<ActionResult> Me()
        {
            UserModelSecuredDto userModel = default;
            try
            {
                var currentPrincipal = this.User;
                if (currentPrincipal?.Identity != null)
                {
                    var login = ClaimsHelper.GetPrincipalClaimsByName(currentPrincipal, ClaimNames.NAME).FirstOrDefault();
                    if (login != null)
                    {
                        var dbUser = await _service.Get(login.Value);
                        if (dbUser != null)
                        {
                            userModel = dbUser.ToDomainSecure();
                            userModel.HasLogin = !string.IsNullOrEmpty(dbUser.Login);
                            userModel.HasPassword = !string.IsNullOrEmpty(dbUser.Password);
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, this.User);
                Unauthorized();
            }

            return Ok(OperationResult<UserModelSecuredDto>.CreateOk(userModel));
        }

        /// <summary>
        /// Updates current user profile
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Profile of the logged in user</response>
        /// <response code="401">Could not establish user identity. The session token might be invalid.</response>
        [HttpPatch("me")]
        [BearerCheckFilter]
        [Authorize(Roles = "admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(OperationResult<UserModelSecuredDto>), 200)]
        [ProducesResponseType(typeof(UnauthorizedResult), 401)]
        public async Task<ActionResult> UpdateMe(UserModelDto model)
        {
            try
            {
                var currentPrincipal = this.User;
                if (currentPrincipal?.Identity != null)
                {
                    var validator = new UserModelValidator();
                    await validator.ValidateAndThrowAsync(model);
                    var updated = await _service.Update(model);
                    return Ok(OperationResult<UserModelSecuredDto>.CreateOk(updated));
                }

                return Unauthorized();
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to update User Model. {@User}, {@Exception}", this.User, e);
            }

            return Unauthorized();
        }


        /// <summary>
        /// Creates new user account
        /// </summary>
        /// <param name="model">Account model</param>
        /// <returns>Created model with user id or error</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpPost("registration")]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] UserModelDto model)
        {
            var validator = new UserModelValidator();
            if (model == null)
            {
                return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelError));
            }
            try
            {
                await validator.ValidateAndThrowAsync(model);
                var validationResult = await validator.ValidateAsync(model);
                if (validationResult.IsValid)
                {
                    var existUser = await _service.Get(model.Login);
                    if (existUser == null)
                    {
                        var created = await _service.Create(model);
                        var response = new LoginResponse
                        {
                            RefreshToken = created.RefreshToken,
                            AccessToken = created.AccessToken
                        };
                        return Ok(OperationResult<LoginResponse>.CreateOk(response));
                    }
                    return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelExistError));
                }
                return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelError, validationResult.Errors));
            }
            catch (Exception e)
            {
                var errorDesc = $"Error creating new user account {e.Message}";
                _logger.LogError(errorDesc);
                return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelAddError, errorDesc));
            }
        }

        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="model">User model</param>
        /// <returns></returns>
        /// <response code="200">Model accepted and updated</response>
        /// <response code="400">See error code</response>
        [HttpPost("recovery")]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(OperationResult<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetPassword([FromBody] UserModelUpdatePasswordDto model)
        {
            var validator = new ResetPasswordValidator();
            var validationResult = await validator.ValidateAsync(model);
            if (validationResult.IsValid)
            {
                var updated = await _service.ResetPassword(model);
                if (updated != null)
                {
                    var response = new LoginResponse
                    {
                        RefreshToken = updated.RefreshToken,
                        AccessToken = updated.AccessToken
                    };
                    return Ok(OperationResult<LoginResponse>.CreateOk(response));
                }
                return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelEditError));
            }
            return BadRequest(OperationResult<LoginResponse>.CreateFail(ErrorCodes.ModelError, validationResult.Errors));
        }

        /// <summary>
        /// Initializes password recovery sequence and sends the password recovery code to the user's e-mail.
        /// </summary>
        /// <param name="login">
        /// User login to initialize recovery sequence
        /// </param>
        /// <response code="200">The recovery token was issued and an e-mail was successfully sent</response>
        /// <response code="400">Internal error during recovery procedure, model in invalid or user does not exist (see error code for details)</response>
        [HttpGet]
        [Route("recovery/{login}")]
        [ProducesResponseType(typeof(OperationResult), 200)]
        [ProducesResponseType(typeof(OperationResult), 400)]
        public async Task<ActionResult> GetRecoveryCode([FromRoute] string login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return BadRequest(new OperationResult
                {
                    IsSuccess = false,
                    ErrorCode = ErrorCodes.ModelError,
                    ErrorData = "Login is empty"
                });
            }
            try
            {
                var user = (await _service.Get(login))?.ToDomain();
                if (user != null)
                {
                    var restoreRequestResult = await _service.RequestPasswordRestore(user);
                    if (!restoreRequestResult)
                    {
                        _logger.LogError("Failed to generate recovery code for user. {@User}", user);
                    }
                }
                return Ok(new OperationResult { IsSuccess = true });
            }
            catch
            {
                _logger.LogError("Unexpected exception while trying to send recovery code. {@Login}", login);
                return Ok(new OperationResult { IsSuccess = true });
            }
        }
    }
}
