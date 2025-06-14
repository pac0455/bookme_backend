using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.NegocioDTO;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface INegocioService
    {
        //POST
        Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios, string usuarioId);
        Task<(bool Success, string Message)> AddAsync(Negocio negocio, string usuarioId);
        //UPDATE
        Task<(bool Success, string Message)> UpdateAsync(Negocio negocio, string usuarioId);
        Task<(bool Success, string Message)> UpdateByNombreAsync(Negocio negocio, string usuarioId);

        //GET
        Task<(bool Success, string Message, List<Negocio> negocios)> GetNegociosByUserId(string userId);
        Task<(bool Success, string Message, List<Reserva> Reservas)> GetReservasByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosByNegocioIdAsync(int negocioId);
        //Task<(bool Success, string Message, List<ReservaDetalladaDto> Reservas)> GetReservasDetalladasByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, Negocio? Negocio)> UpdateNegocioImagenAsync(int negocioId, Imagen imagen);

        Task<(bool Success, string Message, byte[]? ImageBytes, string ContentType)> GetNegocioImagenAsync(int negocioId);
        Task<(bool Success, string Message, List<NegocioCardCliente> Negocios)> GetNegociosParaClienteAsync(Ubicacion? posicionUsuario);
        Task<(bool Success, string Message, NegocioCardCliente? Negocio)> GetNegocioParaClienteAsync(int negocioId, Ubicacion? ubicacionUser);
        Task<(bool Success, string Message, List<NegocioResponseAdminDTO> negocios)> GetAllNegocios();
        Task<(bool Success, string Message)> BloquearNegocioAsync(int negocioId, string usuarioId);
        Task<(bool Success, string Message)> DesbloquearNegocioAsync(int negocioId, string usuarioId);


        //Delete
        Task<(bool Success, string Message)> DeleteAsync(int negocioId, string usuarioId, string motivo);
    }
}
