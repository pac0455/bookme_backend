using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IHorarioService
    {
        Task<(bool Success, string Message)> AddRangeAsync(List<Horarios> horarios);
    }
}
