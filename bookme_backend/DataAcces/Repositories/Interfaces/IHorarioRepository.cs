using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.Repositories.Interfaces
{
    public interface IHorarioRepository: IRepository<Horario>
    {
        Task<bool> AddAllAsync(List<Horario> horarios);
    }
}
