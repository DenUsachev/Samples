using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Samshit.WebUtils;

namespace Campaigns.Api.Web.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected virtual Uri BuildRedirectUri(string forwardRoute)
        {
            var builder = new UriBuilder();
            builder.Scheme = Request.Scheme;
            builder.Host = Request.Host.Host;

            if (Request.Host.Port.HasValue)
                builder.Port = Request.Host.Port.Value;

            builder.Path = Request.Path.Value.Contains("/callback")
                ? Request.Path.Value
                : Request.Path.Value + "/callback";
            builder.Query = $"?forward={forwardRoute}";
            return builder.Uri;
        }

        protected virtual Uri BuildFetchUrl(object id)
        {
            var builder = new UriBuilder { Scheme = Request.Scheme, Host = Request.Host.Host };
            if (Request.Host.Port.HasValue)
            {
                builder.Port = Request.Host.Port.Value;
            }

            builder.Path = $"{Request.Path.Value}/{id}";
            return builder.Uri;
        }

        protected bool TryPassSessionCheck(out string login, out long id)
        {
            login = null;
            id = 0;
            var currentPrincipal = User;
            if (currentPrincipal?.Identity != null)
            {
                Claim loginObject = ClaimsHelper.GetPrincipalClaimsByName(currentPrincipal, "name").FirstOrDefault();
                if (loginObject != null)
                {
                    login = loginObject.Value;
                }

                Claim idObject = ClaimsHelper.GetPrincipalClaimsByName(currentPrincipal, "user_id").FirstOrDefault();
                if (idObject != null && long.TryParse(idObject.Value, out long userId))
                {
                    id = userId;
                }

                if (login != null && id > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}