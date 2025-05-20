using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.BLL.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly IRepository<Horario> _horarioRepo;
        private readonly IRepository<Negocio> _negocioRepo;

        public HorarioService(IRepository<Horario> horarioRepository, IRepository<Negocio> negocioRepo)
        {
            _horarioRepo = horarioRepository;
            _negocioRepo = negocioRepo;
        }


        public async Task<(bool Success, string Message)> AddRangeAsync(List<Horario> horarios)
        {
            try
            {
                // Validar que todos los negocios existen
                foreach (var horario in horarios)
                {
                    var negocio = await _negocioRepo.GetByIdAsync(horario.IdNegocio);
                    if (negocio == null)
                    {
                        return (false, $"Negocio con ID {horario.IdNegocio} no existe.");
                    }
                }

                // Si todos los negocios son válidos, agregamos
                await _horarioRepo.AddRangeAsync(horarios);
                await _horarioRepo.SaveChangesAsync();
                return (true, "Horarios añadidos correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al insertar horarios: {ex.Message}");
            }
        }
    }

}
