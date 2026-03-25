using backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetApiHealth()
        {
            return Ok(new
            {
                status = "success",
                message = "API is healthy"
            });
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDb()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(500, new
                    {
                        status = "error",
                        message = "Database connection failed"
                    });
                }

                return Ok(new
                {
                    status = "success",
                    message = "Database connection is healthy"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Database connection failed",
                    details = ex.Message
                });
            }
        }

    }
}