using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.DTOs.Tickets
{
    public class CreateTicketRequestDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Description { get; set; } = null!;

        [Required]
        [EnumDataType(typeof(TicketPriority))]
        public TicketPriority Priority { get; set; } 

        [Required]
        public int CategoryId { get; set; }
    }
}