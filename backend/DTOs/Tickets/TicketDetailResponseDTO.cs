using backend.Enums;

namespace backend.DTOs.Tickets
{
    public class TicketDetailResponseDTO
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = null!;

        public int? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}