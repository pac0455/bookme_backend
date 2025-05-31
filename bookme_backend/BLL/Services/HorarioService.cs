using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly IRepository<Horario> _horarioRepo;
        private readonly IRepository<Negocio> _negocioRepo;
        private readonly IRepository<Reserva> _reservaRepo;
        private readonly IRepository<Servicio> _servicioRepo;
        private readonly ILogger<HorarioService> _logger;



        public HorarioService(
            IRepository<Horario> horarioRepository,
            IRepository<Negocio> negocioRepo,
            IRepository<Servicio> servicioRepo,
            ILogger<HorarioService> logger,
        IRepository<Reserva> reservaRepo) 
        {
            _horarioRepo = horarioRepository;
            _negocioRepo = negocioRepo;
            _servicioRepo = servicioRepo;
            _reservaRepo = reservaRepo;
            _logger = logger;
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

        public async Task<(bool Success, string Message, List<Horario> horarios)> GetHorariosByNegocioId(int negocioId)
        {
            List<Horario> horarios = new List<Horario>();
            try
            {
                horarios = await _horarioRepo.GetWhereAsync(x => x.IdNegocio == negocioId);

            }
            catch (Exception ex)
            {
                return (false, $"Error desconocido: {ex.StackTrace}", new List<Horario>());
            }

            return (true, "", horarios);
        }

        public async Task<(bool Succes, string Message, List<Horario> horario)> GetHorarioServicioSinReservaByNegocioID(int negocioId, int servicoId, DateOnly date)
        {
            _logger.LogInformation("Iniciando GetHorarioServicioSinReservaByNegocioID con negocioId={NegocioId}, servicoId={ServicioId}, date={Date}", negocioId, servicoId, date);

            try
            {
                _logger.LogInformation("Obteniendo servicio con ID {ServicioId}", servicoId);
                var servicio = await _servicioRepo.GetByIdAsync(servicoId);
                if (servicio == null)
                {
                    _logger.LogWarning("Servicio con ID {ServicioId} no encontrado", servicoId);
                    return (false, "Servicio no encontrado", new List<Horario>());
                }

                _logger.LogInformation("Obteniendo horarios del negocio con ID {NegocioId}", negocioId);
                var horarios = (await GetHorariosByNegocioId(negocioId)).horarios;
                _logger.LogInformation("Total horarios obtenidos: {Count}", horarios.Count);

                string diaSemanaEsp = date.DayOfWeek switch
                {
                    DayOfWeek.Monday => "Lunes",
                    DayOfWeek.Tuesday => "Martes",
                    DayOfWeek.Wednesday => "Miércoles",
                    DayOfWeek.Thursday => "Jueves",
                    DayOfWeek.Friday => "Viernes",
                    DayOfWeek.Saturday => "Sábado",
                    DayOfWeek.Sunday => "Domingo",
                    _ => ""
                };

                _logger.LogInformation("Filtrando horarios para el día de la semana: {DiaSemana}", diaSemanaEsp);
                var horariosDelDia = horarios.Where(h => h.DiaSemana.Equals(diaSemanaEsp, StringComparison.OrdinalIgnoreCase)).ToList();
                _logger.LogInformation("Horarios filtrados para el día: {Count}", horariosDelDia.Count);

                var bloques = new List<Horario>();

                foreach (var horario in horariosDelDia)
                {
                    var horaInicio = horario.HoraInicio;
                    var horaFin = horario.HoraFin;
                    var duracion = TimeSpan.FromMinutes(servicio.DuracionMinutos);

                    if (duracion.TotalMinutes <= 0)
                    {
                        _logger.LogWarning("Duración del servicio inválida: {DuracionMinutos}", servicio.DuracionMinutos);
                        return (false, "Duración del servicio inválida", new List<Horario>());
                    }

                    var totalMinutes = (horaFin - horaInicio).TotalMinutes;
                    var bloquesCount = (int)(totalMinutes / duracion.TotalMinutes);

                    for (int i = 0; i < bloquesCount; i++)
                    {
                        var bloqueInicio = horaInicio.Add(TimeSpan.FromMinutes(i * duracion.TotalMinutes));
                        var bloqueFin = bloqueInicio.Add(duracion);

                        _logger.LogInformation("Creando bloque: HoraInicio={HoraInicio}, HoraFin={HoraFin}", bloqueInicio, bloqueFin);

                        bloques.Add(new Horario
                        {
                            IdNegocio = horario.IdNegocio,
                            DiaSemana = horario.DiaSemana,
                            HoraInicio = bloqueInicio,
                            HoraFin = bloqueFin
                        });
                    }
                }



                _logger.LogInformation("Obteniendo reservas para negocioId={NegocioId}, fecha={Date}", negocioId, date);
                var reservas = await _reservaRepo.GetWhereAsync(r =>
                    r.NegocioId == negocioId &&
                    r.Fecha == date &&
                    r.Estado.Equals(EstadoReserva.Confirmada)
                );
                _logger.LogInformation("Reservas obtenidas: {Count}", reservas.Count);

                var antesCount = bloques.Count;
                bloques.RemoveAll(bloque =>
                    reservas.Any(r => r.HoraInicio < bloque.HoraFin && r.HoraFin > bloque.HoraInicio)
                );
                _logger.LogInformation("Bloques eliminados por reservas: {Eliminados}", antesCount - bloques.Count);

                _logger.LogInformation("Finalizando GetHorarioServicioSinReservaByNegocioID con éxito");
                return (true, "Horarios disponibles generados correctamente", bloques);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetHorarioServicioSinReservaByNegocioID");
                return (false, ex.Message, new List<Horario>());
            }
        }
    }
}
