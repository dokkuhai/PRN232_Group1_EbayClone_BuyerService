using Microsoft.AspNetCore.Mvc;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("healthy");
    }
}
