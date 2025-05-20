using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface INegocioService
    {
        Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios, string usuarioId);
        Task<(bool Success, string Message)> AddAsync(Negocio negocios, string usuarioId);


    }
}
