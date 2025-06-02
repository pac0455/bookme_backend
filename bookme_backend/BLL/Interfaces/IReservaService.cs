using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IReservaService
    {
        Task<(bool Success, string Message, ReservaResponseDTO? reserva)> CrearReservaAsync(ReservaCreateDTO dto);

        Task<(bool Success, string Message, ReservaResponseDTO? reserva)> GetReservaAsync(int reservaId);
        Task<(bool Success, string Message, List<ReservaResponseDTO> reservas)> GetReservasByUserIdAsync(string userId);
    }
}
