using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.Repositories.Interfaces
{
    public interface IHorarioRepository: IRepository<Horarios>
    {
        Task<bool> AddAllAsync(List<Horarios>);
    }
}
