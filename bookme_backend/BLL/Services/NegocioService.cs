using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class NegocioService : INegocioService
    {
        private readonly IRepository<Horarios> _horarioRepo;
        private readonly IRepository<Negocio> _negocioRepo;

        public NegocioService(IRepository<Horarios> horarioRepository, IRepository<Negocio> negocioRepo)
        {
            _horarioRepo = horarioRepository;
            _negocioRepo = negocioRepo;
        }

        public async Task<(bool Success, string Message)> AddAsync(Negocio negocios)
        {
            try
            {
                await _negocioRepo.AddAsync(negocios);
                // Si todos los negocios son válidos, agregamos
                await _negocioRepo.SaveChangesAsync();
                return (true, "Negocio añadido correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al insertar horarios: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios)
        {
            try
            {
                foreach (var negocio in negocios)
                {
                    await _negocioRepo.AddAsync(negocio);
                }
                return (true, "Negocios añadidos correctamente.");
            }
            catch (Exception ex) {

                return (false, $"Error al insertar horarios: {ex.Message}");
            }
        }
    }
}
