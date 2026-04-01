using backend.DTOs.Tickets;

namespace backend.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDTO> CreateTicketAsync(int userId, CreateTicketRequestDTO request);
        Task<List<TicketListItemResponseDTO>> GetTicketsAsync(int userId);
        Task<TicketDetailResponseDTO> GetMyTicketDetailAsync(int userId, int ticketId);
    }
}