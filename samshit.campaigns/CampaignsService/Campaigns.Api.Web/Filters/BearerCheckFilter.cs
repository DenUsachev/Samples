using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Campaigns.Api.Web.Filters
{
    public class BearerCheckFilter : Attribute, IAsyncAuthorizationFilter
    {
        private const string HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //todo: call AuthGateway to validate
            return Task.CompletedTask;
            //if (context.HttpContext.RequestServices.GetService(typeof(IUserService)) is IUserService svc)
            //{
            //    if (context.HttpContext.Request.Headers.TryGetValue(HEADER_NAME, out StringValues headers)
            //    && headers.Count > 0)
            //    {
            //        var tokenValue = headers[0].Replace(BEARER_PREFIX, string.Empty);
            //        var tokenExists = await svc.TokenExists(tokenValue);
            //        if (!tokenExists)
            //        {
            //            context.Result = new UnauthorizedResult();
            //        }
            //    }
            //}
        }
    }
}
