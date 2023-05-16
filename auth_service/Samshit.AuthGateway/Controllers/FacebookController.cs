using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Facebook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.DataModels.UserDomain;
using Samshit.DbAccess.Postgre;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Route("auth/v1/facebook")]
    public class FacebookController : AuthControllerBase
    {
        public override string ApplicationId => "245501853536913";
        public override string ApplicationSecret => "47bdee0bb1a27aa02a474fcf1748ce8e";

        private readonly IUserService _svc;
        private readonly ILogger<FacebookController> _logger;
        private readonly IHttpContextAccessor _httpAccessor;

        public FacebookController(IUserService svc,
            ILogger<FacebookController> logger, IHttpContextAccessor httpAccessor)
        {
            _svc = svc;
            _logger = logger;
            _httpAccessor = httpAccessor;
        }

        [HttpGet]
        public ActionResult Login([FromQuery] string forward)
        {
            if (string.IsNullOrEmpty(forward))
            {
                _logger.LogError("Auth sequence failed: forward parameter missing.");
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
            var callbackUri = BuildRedirectUri(forward);
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = ApplicationId,
                client_secret = ApplicationSecret,
                redirect_uri = callbackUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return new RedirectResult(loginUrl.AbsoluteUri);
        }

        [HttpGet("callback")]
        [ProducesResponseType(typeof(OkResult), 200)]
        [ProducesResponseType(typeof(ErrorData), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Callback(string code, string forward)
        {
            dynamic result = default;
            try
            {
                _logger.LogDebug("Trying to authenticate via Facebook with token: {code}", code);
                var callbackUri = BuildRedirectUri(forward);
                var facebook = new FacebookClient();
                result = facebook.Post("oauth/access_token", new
                {
                    client_id = ApplicationId,
                    client_secret = ApplicationSecret,
                    redirect_uri = callbackUri.AbsoluteUri,
                    code
                });

                facebook.AccessToken = result.access_token;
                dynamic me = facebook.Get("me?fields=first_name,last_name,email,id");
                var socialId = (string)me.id;
                if (!string.IsNullOrEmpty(socialId))
                {
                    var loggedInUser = await _svc.LoginWithSocialId(
                        new SocialUserModel
                        {
                            Id = me.id,
                            Email = me.email,
                            FirstName = me.first_name,
                            LastName = me.last_name
                        }, AccountType.Facebook);
                    var tokenCookieOpts = new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(1), MaxAge = TimeSpan.FromMinutes(2) };

                    _httpAccessor.HttpContext.Response.Cookies.Append("rtk", loggedInUser.RefreshToken, tokenCookieOpts);
                    _httpAccessor.HttpContext.Response.Cookies.Append("atk", loggedInUser.AccessToken, tokenCookieOpts);
                    
                    var returnUrlBuilder = new UriBuilder();
                    returnUrlBuilder.Scheme = Request.Scheme;
                    returnUrlBuilder.Host = Request.Host.Host;
                    if (Request.Host.Port.HasValue)
                        returnUrlBuilder.Port = Request.Host.Port.Value;
                    returnUrlBuilder.Path = forward;
                    return Redirect(returnUrlBuilder.Uri.AbsoluteUri);
                }

                _logger.LogError("Bad data from social network. Social network access result {@Result}:",
                    (object)result);
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
            catch (FacebookOAuthException fex)
            {
                _logger.LogWarning(fex.Message, "Facebook API Exception. {@AuthCode}", code);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to log in user by social network. Social network access result {@Result}:", (object)result);
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
            return Unauthorized();
        }
    }
}