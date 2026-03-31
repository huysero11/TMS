using backend.DTOs.Tickets;

namespace backend.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDTO> CreateTicketAsync(int userId, CreateTicketRequestDTO request);
    }
}