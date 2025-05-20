using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class NegocioService(
        IRepository<Horario> horarioRepo,
        IRepository<Negocio> negocioRepo,
        IRepository<Suscripcion> subcripcionesRepo
        ) : INegocioService
    {

        private readonly IRepository<Negocio> _negocioRepo = negocioRepo;
        private readonly IRepository<Suscripcion> _subcripcionesRepo = subcripcionesRepo;
        private readonly IRepository<Horario> _horarioRepo = horarioRepo;



        //Crea un negocio y lo relaciona con el usuario mediante una subcripción
        public async Task<(bool Success, string Message)> AddAsync(Negocio negocio, string usuarioId)
        {
            try
            {
                await _negocioRepo.AddAsync(negocio);
                await _negocioRepo.SaveChangesAsync();

                foreach (var horario in negocio.HorariosAtencion)
                {
                    horario.Id = 0; // <-- Solución clave
                    horario.IdNegocio = negocio.Id;
                    await _horarioRepo.AddAsync(horario);
                }

                await _horarioRepo.SaveChangesAsync();

                var suscripcion = new Suscripcion
                {
                    FechaSuscripcion = DateTime.Now,
                    IdNegocio = negocio.Id,
                    IdUsuario = usuarioId,
                    RolNegocio = ERol.NEGOCIO.ToString(),
                };
                await _subcripcionesRepo.AddAsync(suscripcion);
                await _subcripcionesRepo.SaveChangesAsync();

                return (true, "Negocio añadido correctamente.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : string.Empty;
                return (false, $"Error al insertar negocio: {ex.Message}{innerMessage}");
            }
        }






        public async Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios, string usuarioId)
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
