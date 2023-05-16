using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Samshit.AuthGateway.Services;
using Samshit.DbAccess.Postgre;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Route("auth/v1/google")]
    public class GoogleController : AuthControllerBase
    {
        public override string ApplicationId => "881243615171-o38roc0vblfqct020l39onlk2t6mcgeh.apps.googleusercontent.com";
        public override string ApplicationSecret => "K46RY55JTyk7gJ1roZs-2LWP";

        private ILogger<GoogleController> _logger;
        private readonly SamshitDbContext _ctx;

        public GoogleController(DbContext ctx,
            ILogger<GoogleController> logger)
        {
            _logger = logger;
            _ctx = (SamshitDbContext)ctx;
        }


        [HttpGet]
        public ActionResult Login()
        {
            return Unauthorized();
            //var google = new GoogleAuthProvider
            //{
            //    ClientId = ApplicationId,
            //    AccessFlags = GoogleScope.Email + "+" + GoogleScope.Profile,
            //    Secret = ApplicationSecret
            //};
            ////Session["googleAuthenticator"] = google;
            //var url = google.GetServiceUri(BuildRedirectUri());
            //return new RedirectResult(url);
        }

        [HttpGet("callback")]
        public ActionResult Callback(string code)
        {
            try
            {
                _logger.LogDebug("Trying to authenticate with token: {code}", code);
                return Ok($"Ok from google: {code}");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}