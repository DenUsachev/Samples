using Microsoft.AspNetCore.Mvc;
using System;

namespace Samshit.AuthGateway.Controllers
{
    public class AuthControllerBase : ControllerBase
    {
        protected virtual Uri BuildRedirectUri(string forwardRoute)
        {
            var builder = new UriBuilder();
            builder.Scheme = Request.Scheme;
            builder.Host = Request.Host.Host;

            if (Request.Host.Port.HasValue)
                builder.Port = Request.Host.Port.Value;

            builder.Path = Request.Path.Value.Contains("/callback") ? Request.Path.Value : Request.Path.Value + "/callback";
            builder.Query = $"?forward={forwardRoute}";
            return builder.Uri;
        }

        public virtual string ApplicationId { get; }
        public virtual string ApplicationSecret { get; }
    }
}
