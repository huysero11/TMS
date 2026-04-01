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
    
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMyTicketDetail([FromRoute] int id)
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

            var ticketDetail = await _ticketService.GetMyTicketDetailAsync(userId, id);

            if (ticketDetail is null)
            {
                return NotFound(new
                {
                    status = "fail",
                    message = "Ticket not found"
                });
            }
            
            return Ok(new
            {
                status = "success",
                message = "Ticket detail retrieved successfully",
                data = ticketDetail
            });
        }
    
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMyTicket([FromRoute] int id, [FromBody] UpdateTicketRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    status = "fail",
                    message = "Invalid user ID claim"
                });
            }

            try
            {
                var updatedTicket = await _ticketService.UpdateMyTicketAsync(userId, id, request);
                return Ok(new
                {
                    status = "success",
                    message = "Ticket updated successfully",
                    data = updatedTicket
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    status = "fail",
                    message = ex.Message
                });
            }
        }
    
        [HttpPatch("{id:int}/close")]
        public async Task<IActionResult> CloseMyTicket([FromRoute] int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    status = "fail",
                    message = "Invalid user ID claim"
                });
            }

            try
            {
                var closedTicket = await _ticketService.CloseMyTicketAsync(userId, id);
                return Ok(new
                {
                    status = "success",
                    message = "Ticket closed successfully",
                    data = closedTicket
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    status = "fail",
                    message = ex.Message
                });
            }
        }

        [HttpPatch("{id:int}/cancel")]    
        public async Task<IActionResult> CancelMyTicket([FromRoute] int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    status = "fail",
                    message = "Invalid user ID claim"
                });
            }

            try
            {
                var canceledTicket = await _ticketService.CancelMyTicketAsync(userId, id);
                return Ok(new
                {
                    status = "success",
                    message = "Ticket canceled successfully",
                    data = canceledTicket
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    status = "fail",
                    message = ex.Message
                });
            }
        }
    }
}