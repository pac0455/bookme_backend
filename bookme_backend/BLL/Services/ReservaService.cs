using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.DTO.Usuario;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.BLL.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IRepository<Reserva> _reservaRepo;
        private readonly IRepository<Pago> _pagoRepo;
        private readonly IRepository<Negocio> _negocioRepo;
        private readonly IRepository<Servicio> _servicioRepo;
        private readonly IRepository<Usuario> _usuarioRepo;

        private readonly IPasarelaSimulada _pasarelaSimulada;
        private readonly UserManager<Usuario> _userManager;

        public ReservaService(
            IRepository<Reserva> reservaRepo,
            IRepository<Pago> pagoRepo,
            IPasarelaSimulada pasarelaSimulada,
            IRepository<Negocio> negocioRepo,
            IRepository<Servicio> servicioRepo,
            IRepository<Usuario> usuarioRepo,
            UserManager<Usuario> userManager)
        {
            _reservaRepo = reservaRepo;
            _pagoRepo = pagoRepo;
            _pasarelaSimulada = pasarelaSimulada;
            _negocioRepo = negocioRepo;
            _servicioRepo = servicioRepo;
            _userManager = userManager;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<(bool Success, string Message, ReservaResponseDTO? reserva)> CrearReservaAsync(ReservaCreateDTO dto)
        {
            if (dto == null)
                return (false, "Los datos de la reserva son inválidos.", null);

            var negocioExiste = await _negocioRepo.Exist(n => n.Id == dto.NegocioId);
            if (!negocioExiste)
                return (false, "El negocio especificado no existe.", null);

            var servicio = await _servicioRepo.GetByIdAsync(dto.ServicioId);
            if (servicio == null)
                return (false, "El servicio especificado no existe.", null);

            var reserva = new Reserva
            {
                NegocioId = dto.NegocioId,
                UsuarioId = dto.UsuarioId,
                Fecha = DateOnly.FromDateTime(dto.Fecha),
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Estado = EstadoReserva.Pendiente,
                FechaCreacion = DateTime.UtcNow,
                ServicioId = dto.ServicioId
            };

            await _reservaRepo.AddAsync(reserva);
            await _reservaRepo.SaveChangesAsync();

            Pago pago;

            switch (dto.Pago.MetodoPago)
            {
                case MetodoPago.Tarjeta:
                case MetodoPago.PayPal:
                    // Pago en línea: procesar con pasarela
                    var resultadoPago = await _pasarelaSimulada.ProcesarPagoAsync(dto.Pago);

                    pago = new Pago
                    {
                        ReservaId = reserva.Id,
                        Monto = dto.Pago.Monto,
                        EstadoPago = resultadoPago.Exitoso ? EstadoPago.Confirmado : EstadoPago.Fallido,
                        MetodoPago = dto.Pago.MetodoPago,
                        RespuestaPasarela = resultadoPago.Mensaje,
                        FechaPago = resultadoPago.Exitoso ? DateTime.UtcNow : null,
                        Moneda = dto.Pago.Moneda ?? "EUR"
                    };
                    break;

                default:
                    // Pago manual: no usar pasarela
                    pago = new Pago
                    {
                        ReservaId = reserva.Id,
                        Monto = dto.Pago.Monto,
                        EstadoPago = EstadoPago.Pendiente,
                        MetodoPago = dto.Pago.MetodoPago,
                        RespuestaPasarela = "Pago en espera de confirmación manual",
                        FechaPago = null,
                        Moneda = dto.Pago.Moneda ?? "EUR"
                    };
                    break;
            }

            await _pagoRepo.AddAsync(pago);
            await _pagoRepo.SaveChangesAsync();

            // Usar el método estático para mapear
            var reservaDto = ReservaMapper.FromEntity(reserva, servicio, pago);

            return (true, "Reserva creada correctamente", reservaDto);
        }

        public async Task<(bool Success, string Message)> CancelarReservaAsync(int reservaId, string usuarioId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId);

            if (reserva == null)
                return (false, "Reserva no encontrada.");

            if (reserva.UsuarioId != usuarioId)
                return (false, "No tienes permiso para cancelar esta reserva.");

            if (reserva.Estado == EstadoReserva.Cancelada)
                return (false, "La reserva ya ha sido cancelada.");

            if (reserva.Estado == EstadoReserva.Finalizada)
                return (false, "No se puede cancelar una reserva ya finalizada.");

            // Obtener pago relacionado
            var pagos = await _pagoRepo.GetWhereAsync(n => n.ReservaId ==  reservaId);
            var pago = pagos.FirstOrDefault();

            if (pago != null && pago.EstadoPago == EstadoPago.Confirmado)
            {
                // Solo hacemos "reembolso" simulado si método es online
                if (pago.MetodoPago == MetodoPago.Tarjeta || pago.MetodoPago == MetodoPago.PayPal)
                {
                    // Simular reembolso, por ejemplo llamar a pasarela de reembolso
                    bool reembolsoExitoso = await SimularReembolsoAsync(pago);

                    if (!reembolsoExitoso)
                        return (false, "Error al procesar el reembolso.");

                    // Actualizar pago a estado fallido o reembolsado si tienes ese estado
                    pago.EstadoPago = EstadoPago.Fallido; // O nuevo estado Reembolsado si tienes
                    _pagoRepo.Update(pago);
                }
                else
                {
                    // Para efectivo o transferencia, solo cancelamos sin reembolso automático
                }
            }

            reserva.Estado = EstadoReserva.Cancelada;
            _reservaRepo.Update(reserva);
            await _reservaRepo.SaveChangesAsync();

            return (true, "Reserva cancelada correctamente.");
        }

        private async Task<bool> SimularReembolsoAsync(Pago pago)
        {
            // Aquí va la lógica para simular o llamar a pasarela de reembolso
            await Task.Delay(500); // simular espera
            return true; // siempre exitoso en simulación
        }


        public async Task<(bool Success, string Message, ReservaResponseDTO? reserva)> GetReservaAsync(int reservaId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId);
            if (reserva == null)
                return (false, "Reserva no encontrada", null);

            var servicio = await _servicioRepo.GetByIdAsync(reserva.ServicioId);
            if (servicio == null)
                return (false, "Servicio asociado no encontrado", null);

            var pagos = await _pagoRepo.GetWhereAsync(p => p.ReservaId == reservaId);
            var pago = pagos.FirstOrDefault();

            var reservaDTO = ReservaMapper.FromEntity(reserva, servicio, pago);

            return (true, "Reserva encontrada", reservaDTO);
        }


        public async Task<(bool Success, string Message, List<ReservaResponseDTO> reservas)> GetReservasByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "El ID de usuario es inválido.", new List<ReservaResponseDTO>());

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado.", new List<ReservaResponseDTO>());

            var reservas = await _reservaRepo.GetWhereAsync(r => r.UsuarioId == userId);
            var reservaDtos = new List<ReservaResponseDTO>();

            foreach (var reserva in reservas.OrderByDescending(r => r.FechaCreacion))
            {
                var servicio = await _servicioRepo.GetByIdAsync(reserva.ServicioId);
                if (servicio == null)
                    continue; 

                var pagos = await _pagoRepo.GetWhereAsync(p => p.ReservaId == reserva.Id);
                var pago = pagos.FirstOrDefault();


                var fechaFinReserva = reserva.Fecha.ToDateTime(reserva.HoraFin);
                bool esFinalizada = DateTime.Now > fechaFinReserva;

                var estadoFinal = reserva.Estado == EstadoReserva.Cancelada
                    ? EstadoReserva.Cancelada
                    : (esFinalizada ? EstadoReserva.Finalizada : reserva.Estado);

                reservaDtos.Add(ReservaMapper.FromEntity(reserva, servicio, pago));
            }

            return (true, "Reservas del usuario encontradas", reservaDtos);
        }
        public async Task<(bool Success, string Message, ReservaResponseDTO? reserva)> CancelarReservaByNegocioId(int reservaId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId);
            if (reserva == null)
                return (false, "Reserva no encontrada.", null);

            if (reserva.Estado == EstadoReserva.Cancelada)
                return (false, "La reserva ya está cancelada.", null);

            if (reserva.Estado == EstadoReserva.Finalizada)
                return (false, "No se puede cancelar una reserva finalizada.", null);

            reserva.Estado = EstadoReserva.Cancelada;
            _reservaRepo.Update(reserva);
            await _reservaRepo.SaveChangesAsync();

            var servicio = await _servicioRepo.GetByIdAsync(reserva.ServicioId);
            var pagos = await _pagoRepo.GetWhereAsync(p => p.ReservaId == reservaId);
            var pago = pagos.FirstOrDefault();

            var reservaDto = ReservaMapper.FromEntity(reserva, servicio!, pago);

            return (true, "Reserva cancelada correctamente.", reservaDto);
        }

        public async Task<(bool Success, string Message, List<ReservaResponseNegocioDTO> reservas)> GetReservaNegocioByNegocioId(int negocioId)
        {
            try
            {
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.Usuario,
                    r => r.Servicio,
                    r => r.Pago
                );

                if (reservas == null || !reservas.Any())
                    return (true, "", new List<ReservaResponseNegocioDTO>());

                // Conteo reservas por usuario
                var reservasPorUsuario = reservas
                    .GroupBy(r => r.UsuarioId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var resultado = reservas.Select(r =>
                {
                    var dto = ReservaNegocioMapper.FromEntity(r, r.Usuario);
                    dto.NReservasUsuario = reservasPorUsuario.ContainsKey(r.UsuarioId) ? reservasPorUsuario[r.UsuarioId] : 0;
                    return dto;
                }).ToList();

                return (true, "Reservas obtenidas", resultado);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener reservas: {ex.Message}", new List<ReservaResponseNegocioDTO>());
            }
        }

        public async Task<(bool Success, string Message, List<ReservasPorDiaDTO> Data)> GetReservasPorDiaSemanaAsync(int negocioId)
        {
            try
            {
                var reservas = await _reservaRepo.Query()
                    .Where(r => r.NegocioId == negocioId)
                    .ToListAsync();

                // Diccionario para mapear DayOfWeek a inicial en español
                var diasIniciales = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "L" },
            { DayOfWeek.Tuesday, "M" },
            { DayOfWeek.Wednesday, "X" },
            { DayOfWeek.Thursday, "J" },
            { DayOfWeek.Friday, "V" },
            { DayOfWeek.Saturday, "S" },
            { DayOfWeek.Sunday, "D" }
        };

                // Lista fija en orden para garantizar todos los días
                var diasSemana = diasIniciales.Values.ToList();

                var reservasPorDia = reservas
                    .GroupBy(r => r.Fecha.DayOfWeek)
                    .Select(g => new ReservasPorDiaDTO
                    {
                        DiaSemana = diasIniciales[g.Key], // Traducimos al inicial español aquí
                        TotalReservas = g.Count()
                    })
                    .ToList();

                // Crear resultado final asegurando todos los días con su inicial
                var resultadoCompleto = diasSemana
                    .Select(dia => reservasPorDia.FirstOrDefault(r => r.DiaSemana == dia)
                                   ?? new ReservasPorDiaDTO { DiaSemana = dia, TotalReservas = 0 })
                    .ToList();

                return (true, "OK", resultadoCompleto);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener reservas por día: {ex.Message}", new List<ReservasPorDiaDTO>());
            }
        }


        public async Task<(bool Success, string Message)> CambiarEstadoPagoDeReservaAsync(int reservaId, EstadoPago nuevoEstado)
        {
            var pagos = await _pagoRepo.GetWhereAsync(p => p.ReservaId == reservaId);
            var pago = pagos.FirstOrDefault();

            if (pago == null)
                return (false, "Pago no encontrado para la reserva.");

            pago.EstadoPago = nuevoEstado;
            await _pagoRepo.SaveChangesAsync();

            return (true, "Estado del pago actualizado correctamente.");
        }
    }
}
