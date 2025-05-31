using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IReservaService
    {
        Task<(bool Success, string Message, Reserva reserva)> CrearReservaAsync(
            Reserva reserva,
            List<int> serviciosIds,
            Pago? pago = null
        );
        
    }
}
