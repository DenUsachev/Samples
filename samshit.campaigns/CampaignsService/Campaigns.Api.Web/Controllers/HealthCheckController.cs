using Microsoft.AspNetCore.Mvc;

namespace Campaigns.Api.Web.Controllers
{
    [ApiController]
    [Route("campaigns/health-check")]
    [Produces("application/json")]
    public class HealthCheckController : ApiControllerBase
    {

        /// <summary>
        /// Returns the permanent ok result to verify that service is up and running
        /// </summary>
        [HttpGet]
        public ActionResult HealthCheck()
        {
            return NoContent();
        }
    }
}
