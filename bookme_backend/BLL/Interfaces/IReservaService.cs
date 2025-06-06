using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.DTO.Usuario;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IReservaService
    {
        Task<(bool Success, string Message, ReservaResponseDTO? reserva)> CrearReservaAsync(ReservaCreateDTO dto);

        Task<(bool Success, string Message, ReservaResponseDTO? reserva)> GetReservaAsync(int reservaId);
        Task<(bool Success, string Message, List<ReservaResponseDTO> reservas)> GetReservasByUserIdAsync(string userId);
        Task<(bool Success, string Message, List<ReservaResponseNegocioDTO> reservas)> GetReservaNegocioByNegocioId(int negocioId);
        Task<(bool Success, string Message, ReservaResponseDTO? reservas)> CancelarReservaByNegocioId(int reservaId);
        Task<(bool Success, string Message, List<ReservasPorDiaDTO> Data)> GetReservasPorDiaSemanaAsync(int negocioId);
        Task<(bool Success, string Message)> CambiarEstadoPagoDeReservaAsync(int reservaId, EstadoPago nuevoEstado);


    }
}
