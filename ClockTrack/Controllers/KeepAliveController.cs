using Microsoft.AspNetCore.Mvc;

namespace ClockTrack.Controllers
{
    public class KeepAliveController : Controller
    {

        [Route("KeepAlive")]
        [HttpGet]
        public IActionResult KeepAlive()
        {
            return Ok();
        }


    }
}
