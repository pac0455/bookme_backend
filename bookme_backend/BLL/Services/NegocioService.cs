using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class NegocioService(
        IRepository<Horarios> horarioRepository,
        IRepository<Negocio> negocioRepo,
        IRepository<Suscripcione> subcripcionesRepo
        ) : INegocioService
    {

        private readonly IRepository<Negocio> _negocioRepo = negocioRepo;
        private readonly IRepository<Suscripcione> _subcripcionesRepo = subcripcionesRepo;


        //Crea un negocio y lo relaciona con el usuario mediante una subcripción
        public async Task<(bool Success, string Message)> AddAsync(Negocio negocio)
        {
            try
            {
                await _negocioRepo.AddAsync(negocio);
                // Si todos los negocios son válidos, agregamos

                var subcripcion = new Suscripcione
                {
                    FechaSuscripcion = DateTime.Now,
                    IdNegocio = negocio.Id,
                    RolNegocio = ERol.NEGOCIO.ToString(),
                };
                await _subcripcionesRepo.AddAsync(subcripcion);
                await _negocioRepo.SaveChangesAsync();
                await _subcripcionesRepo.SaveChangesAsync();

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
