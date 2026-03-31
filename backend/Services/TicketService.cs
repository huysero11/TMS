using backend.Data;
using backend.DTOs.Tickets;
using backend.Enums;
using backend.Models.Entities;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;

        public TicketService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TicketResponseDTO> CreateTicketAsync(int userId, CreateTicketRequestDTO request)
        {
            // check if category exists
            var category = await _context.TicketCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive);
            if (category is null)
            {
                throw new Exception("Category not found");
            }

            // create ticket
            var ticket = new Ticket
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CategoryId = request.CategoryId,
                CreatedByUserId = userId,
                AssignedToUserId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ClosedAt = null,
                TicketCode = string.Empty
            };

            _context.Tickets.Add(ticket);
            var debugView = _context.ChangeTracker.DebugView.LongView;
            Console.WriteLine(debugView);
            await _context.SaveChangesAsync();

            ticket.TicketCode = GenerateTicketCode(ticket.Id);
            await _context.SaveChangesAsync();

            return new TicketResponseDTO
            {
                Id = ticket.Id,
                TicketCode = ticket.TicketCode,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority,
                Status = ticket.Status,
                CategoryId = category.Id,
                CategoryName = category.Name,
                CreatedByUserId = ticket.CreatedByUserId,
                AssignedToUserId = ticket.AssignedToUserId,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt
            };
        }

        private static string GenerateTicketCode(int ticketId)
        {
            return $"TCK-{ticketId:D6}";
        }
    }

    
}