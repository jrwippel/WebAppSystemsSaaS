using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityTypesApiController : ControllerBase
    {
        private readonly ClockTrackContext _context;

        public ActivityTypesApiController(ClockTrackContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("QuickCreate")]
        public async Task<IActionResult> QuickCreate([FromBody] QuickCreateActivityTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest("Name is required.");

            var activityType = new ActivityType
            {
                Name = request.Name.Trim(),
                Color = string.IsNullOrWhiteSpace(request.Color) ? "#667eea" : request.Color,
                IsActive = true,
                DisplayOrder = 99
            };

            _context.ActivityTypes.Add(activityType);
            await _context.SaveChangesAsync();

            return Ok(new { id = activityType.Id, name = activityType.Name, color = activityType.Color });
        }
    }

    public class QuickCreateActivityTypeRequest
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
