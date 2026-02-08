using BLFramework.Services;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlowAPI.Controllers
{
    /// <summary>
    /// Health check controller for verifying API availability and status.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging health check activities.</param>
        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the current health status of the TaskFlow API.
        /// </summary>
        /// <returns>
        /// An HTTP 200 OK response containing a message indicating the API is running
        /// along with the current UTC timestamp.
        /// </returns>
        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            return Ok(new { message = "TaskFlow API is running", timestamp = DateTime.UtcNow });
        }
    }
}
