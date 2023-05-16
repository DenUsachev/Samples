using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Samshit.AuthGateway.Models;

namespace Samshit.AuthGateway.Controllers
{
    [ApiController]
    [Route("auth/health-check")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public ActionResult Check()
        {
#if DEBUG
            return Ok(new OperationResult
            {
                ErrorCode = null,
                IsSuccess = true,
            });
#else
            if (Request.Headers.TryGetValue("X-Debug", out _))
            {
                return Ok(OperationResult<string>.CreateOk(DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            }

            return NoContent();
#endif
        }

    }
}
