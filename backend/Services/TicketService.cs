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
    
        public async Task<List<TicketListItemResponseDTO>> GetTicketsAsync(int userId)
        {
            var tickets = await _context.Tickets
                    .AsNoTracking()
                    .Where(t => t.CreatedByUserId == userId)
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.CreatedAt)
                    .ThenByDescending(t => t.Id)
                    .Select(t => new TicketListItemResponseDTO
                    {
                        Id = t.Id,
                        TicketCode = t.TicketCode,
                        Title = t.Title,
                        Priority = t.Priority,
                        Status = t.Status,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category.Name,
                        CreatedByUserId = t.CreatedByUserId,
                        AssignedToUserId = t.AssignedToUserId,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        ClosedAt = t.ClosedAt
                    })
                    .ToListAsync();
            return tickets;
            
        }
    
        public async Task<TicketDetailResponseDTO> GetMyTicketDetailAsync(int userId, int ticketId)
        {
            var ticketDetail = await _context.Tickets
                .AsNoTracking()
                .Where(t => t.Id == ticketId && t.CreatedByUserId == userId)
                .Select(t => new TicketDetailResponseDTO
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CreatedByUserId = t.CreatedByUserId,
                    CreatedByUserName = t.CreatedByUser.FullName,
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ClosedAt = t.ClosedAt
                })
                .FirstOrDefaultAsync();       
            return ticketDetail ?? throw new Exception("Ticket not found");   
        }
    
        public async Task<TicketDetailResponseDTO> UpdateMyTicketAsync(int userId, int ticketId, 
                                                                UpdateTicketRequestDTO request)
        {
            var ticket = await _context.Tickets
                                .FirstOrDefaultAsync(t => t.Id == ticketId && t.CreatedByUserId == userId);
            if (ticket is null)
            {
                throw new Exception("Ticket not found");
            }
            
            if (ticket.Status == TicketStatus.Closed || ticket.Status == TicketStatus.Cancelled)
            {
                throw new Exception("Cannot update a closed or cancelled ticket");
            }

            var category = await _context.TicketCategories
                                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive);
            if (category is null)
            {
                throw new Exception("Invalid category");
            }

            ticket.Title = request.Title.Trim();
            ticket.Description = request.Description.Trim();
            ticket.Priority = request.Priority;
            ticket.CategoryId = request.CategoryId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new TicketDetailResponseDTO
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
                CreatedByUserName = string.Empty,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToUserName = null,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt
            };
        }
    }
}