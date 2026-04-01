using System.Security.Claims;
using backend.DTOs.Tickets;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
       private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID claim not found");
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    status = "error",
                    message = "Invalid user ID claim"
                });
            }

            var ticketResponseDto = await _ticketService.CreateTicketAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, new
            {
                status = "success",
                message = "Ticket created successfully",
                data = ticketResponseDto
            });
        }
    
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    status = "fail",
                    message = "Invalid user ID claim"
                });
            }

            var tickets = await _ticketService.GetTicketsAsync(userId);
            return Ok(new
            {
                status = "success",
                message = "Tickets retrieved successfully",
                data = tickets
            });
        }
    }
}