using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class ReservaService
    {
        private readonly IRepository<Reserva> _reservaRepo;
        private readonly IRepository<ReservasServicio> _reservaServicioRepo;
        private readonly IRepository<Pago> _pagoRepo;

        public ReservaService(
            IRepository<Reserva> reservaRepo,
            IRepository<ReservasServicio> reservaServicioRepo,
            IRepository<Pago> pagoRepo)
        {
            _reservaRepo = reservaRepo;
            _reservaServicioRepo = reservaServicioRepo;
            _pagoRepo = pagoRepo;
        }

        public async Task<(bool Success, string Message, Reserva? Reserva)> CrearReservaAsync(Reserva reserva, List<int> servicioIds, Pago? pago = null)
        {
            try
            {
                reserva.FechaCreacion = DateTime.UtcNow;

                // 1. Crear reserva
                await _reservaRepo.AddAsync(reserva);
                await _reservaRepo.SaveChangesAsync(); // Para obtener ID generado
                int reservaId = reserva.Id;

                // 2. Asociar servicios
                var reservasServicios = servicioIds.Select(servicioId => new ReservasServicio
                {
                    ReservaId = reservaId,
                    ServicioId = servicioId
                }).ToList();

                await _reservaServicioRepo.AddRangeAsync(reservasServicios);

                // 3. Crear pago si corresponde
                if (pago != null)
                {
                    pago.ReservaId = reservaId;
                    pago.Creado = DateTime.UtcNow;
                    await _pagoRepo.AddAsync(pago);
                }

                // Guardar cambios
                await _reservaServicioRepo.SaveChangesAsync();
                if (pago != null)
                    await _pagoRepo.SaveChangesAsync();

                // Cargar la reserva creada con sus servicios relacionados (opcional)
                var reservaCreada = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.Id == reservaId,
                    r => r.ReservasServicios);

                return (true, "Reserva creada correctamente", reservaCreada.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la reserva: {ex.Message}", null);
            }
        }
    }
}
