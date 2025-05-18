using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IHorarioService
    {
        Task<Horarios> AddAsync(Horarios horrio);
    }
}
