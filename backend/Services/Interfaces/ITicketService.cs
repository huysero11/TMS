using backend.DTOs.Tickets;

namespace backend.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponseDTO> CreateTicketAsync(int userId, CreateTicketRequestDTO request);
        Task<List<TicketListItemResponseDTO>> GetTicketsAsync(int userId);
        Task<TicketDetailResponseDTO> GetMyTicketDetailAsync(int userId, int ticketId);
        Task<TicketDetailResponseDTO> UpdateMyTicketAsync(int userId, int ticketId, UpdateTicketRequestDTO request);
        Task<TicketDetailResponseDTO> CloseMyTicketAsync(int userId, int ticketId);
        Task<TicketDetailResponseDTO> CancelMyTicketAsync(int userId, int ticketId);
    }
}