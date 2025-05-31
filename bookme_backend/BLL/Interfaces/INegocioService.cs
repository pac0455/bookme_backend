using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface INegocioService
    {
        Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios, string usuarioId);
        Task<(bool Success, string Message)> AddAsync(Negocio negocio, string usuarioId);
        Task<(bool Success, string Message, List<Negocio> negocios)> GetByUserId(string userId);
        Task<(bool Success, string Message)> UpdateAsync(Negocio negocio, string usuarioId);
        Task<(bool Success, string Message)> UpdateByNombreAsync(Negocio negocio, string usuarioId);
        Task<(bool Success, string Message, List<Reserva> Reservas)> GetReservasByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, List<ReservaDetalladaDto> Reservas)> GetReservasDetalladasByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, Negocio? Negocio)> UpdateNegocioImagenAsync(int negocioId, Imagen imagen);

        Task<(bool Success, string Message, byte[]? ImageBytes, string ContentType)> GetNegocioImagenAsync(int negocioId);
        Task<(bool Success, string Message, List<NegocioCardCliente> Negocios)> GetNegociosParaClienteAsync(Ubicacion? posicionUsuario);





    }

}
