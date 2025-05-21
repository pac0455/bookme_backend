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
    }

}
