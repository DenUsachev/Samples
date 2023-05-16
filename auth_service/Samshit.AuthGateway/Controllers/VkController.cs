using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.Interfaces;
using Samshit.AuthGateway.Models;
using Samshit.AuthGateway.Services;
using Samshit.WebUtils;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Samshit.DataModels.ErrorDefinitions;
using Samshit.DataModels.UserDomain;


namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Route("auth/v1/vk")]
    public class VkController : AuthControllerBase
    {
        private const string FLAGS = "friends";
        public override string ApplicationId => "7490909";
        public override string ApplicationSecret => "WVqhFVdKLoZU40Vu8mGf";

        private Nito.AsyncEx.AsyncLock _authGuard = new Nito.AsyncEx.AsyncLock();
        private ILogger<VkController> _logger;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly IUserService _svc;
        private readonly VkAuthProvider _provider;

        public VkController(IUserService service, ILogger<VkController> logger, IHttpContextAccessor httpAccessor)
        {
            _svc = service;
            _logger = logger;
            _httpAccessor = httpAccessor;
            _provider = new VkAuthProvider(ApplicationId, ApplicationSecret) { AccessFlags = FLAGS };
        }

        [HttpGet]
        public ActionResult Login(string forward)
        {
            if (string.IsNullOrEmpty(forward))
            {
                _logger.LogError("Auth sequence failed: forward parameter missing.");
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
            var redirectUri = BuildRedirectUri(forward);
            return Redirect(_provider.GetServiceUri(redirectUri));
        }

        [HttpGet("callback")]
        public async Task<ActionResult> Callback(string code, string forward)
        {
            try
            {
                _logger.LogDebug("Trying to authenticate via VK with token: {code}", code);
                var redirectUri = BuildRedirectUri(forward);
                _provider.RedirectUri = redirectUri.TrimTokenizedString();
                var accessToken = _provider.AcquireToken(code);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    VkUserModel vkUser = _provider.ExecuteQuery(new Uri("https://api.vk.com/method/users.get"));
                    var socialId = vkUser.Id.ToString();
                    using (await _authGuard.LockAsync())
                    {
                        if (!string.IsNullOrEmpty(socialId))
                        {
                            var loggedInUser = await _svc.LoginWithSocialId(
                                new SocialUserModel
                                {
                                    Id = socialId,
                                    Email = null,
                                    FirstName = vkUser.FirstName,
                                    LastName = vkUser.LastName
                                }, AccountType.Vk);
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
                        _logger.LogError("Bad data from social network.");
                        return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
                    };
                }
                _logger.LogError("Bad data from social network. Could not acquire access token.");
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Bad data from social network.");
                return BadRequest(new ErrorData(ErrorCodes.LoginError, Guid.NewGuid()));
            }
        }
    }
}
