using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.Models;
using Microsoft.VisualBasic;

namespace bookme_backend.BLL.Interfaces
{
    public interface IServicioService
    {
        Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosByNegocioIdAsync(int negocioId);
        Task<Servicio> GetServicioByIdAsync(int id);
        Task<(bool Success, string Message, Servicio? Servicio)> AddServicioAsync(ServicioDto servicio);
        Task<(bool Success, string Message)> UpdateServicioAsync(int id, ServicioDto servicio);
        Task<(bool Success, string Message)> DeleteServicioAsync(int id);
        Task<List<Servicio>> GetAllServiciosAsync();
        Task<(bool Success, string Message, List<ServicioDetalleDto> Servicios)> GetServiciosDetalleByNegocioIdAsync(int negocioId);
        Task<(bool Success, string Message, byte[]? ImageBytes, string ContentType)> GetImagenByServicioIdAsync(int servicioId);

    }
}
