using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly IRepository<Horarios> _horarioRepo;
        private readonly IRepository<Negocio> _negocioRepo;

        public HorarioService(IRepository<Horarios> horarioRepository, IRepository<Negocio> negocioRepo)
        {
            _horarioRepo = horarioRepository;
            _negocioRepo = negocioRepo;
        }


        public Task<Horarios> AddAsync(Horarios horario)
        {

            var negocio = _negocioRepo.GetByIdAsync(horario.IdNegocio);
            //Comprobar si el negocio existe
            if (negocio is not null)
            {

            }
            throw new NotImplementedException();
        }
    }

}
