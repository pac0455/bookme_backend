using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Pasarela;

namespace bookme_backend.BLL.Interfaces
{
    public interface IPasarelaSimulada
    {
        Task<ResultadoPasarela> ProcesarPagoAsync(PagoCreateDTO solicitudPago);
    }
}
