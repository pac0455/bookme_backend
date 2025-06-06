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

            if (dto.Pago != null)
            {
                var metodo = dto.Pago.MetodoPago;

                if (metodo == MetodoPago.Tarjeta || metodo == MetodoPago.PayPal)
                {
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
                }
                else
                {
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
                }
            }
            else
            {
                pago = new Pago
                {
                    ReservaId = reserva.Id,
                    Monto = 0,
                    EstadoPago = EstadoPago.Pendiente,
                    MetodoPago = MetodoPago.Efectivo,
                    RespuestaPasarela = "Pago pendiente",
                    FechaPago = null,
                    Moneda = "EUR"
                };
            }

            await _pagoRepo.AddAsync(pago);
            await _pagoRepo.SaveChangesAsync();

            var reservaDto = new ReservaResponseDTO
            {
                Id = reserva.Id,
                NegocioId = reserva.NegocioId,
                UsuarioId = reserva.UsuarioId,
                Fecha = reserva.Fecha,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Estado = reserva.Estado,
                FechaCreacion = reserva.FechaCreacion,
                ServicioId = reserva.ServicioId,
                Servicio = new ServicioDTO
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre
                },
                Pago = new PagoDTO
                {
                    Id = pago.Id,
                    Monto = pago.Monto ?? -1,
                    EstadoPago = pago.EstadoPago,
                    MetodoPago = pago.MetodoPago,
                }
            };

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

            var reservaDTO = new ReservaResponseDTO
            {
                Id = reserva.Id,
                NegocioId = reserva.NegocioId,
                UsuarioId = reserva.UsuarioId,
                Fecha = reserva.Fecha,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Estado = reserva.Estado,
                FechaCreacion = reserva.FechaCreacion,
                ServicioId = reserva.ServicioId,
                Servicio = new ServicioDTO
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre
                },
                Pago = pago != null ? new PagoDTO
                {
                    Id = pago.Id,
                    Monto = pago.Monto ?? 0,
                    EstadoPago = pago.EstadoPago,
                    MetodoPago = pago.MetodoPago,
                } : null
            };

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

                reservaDtos.Add(new ReservaResponseDTO
                {
                    Id = reserva.Id,
                    NegocioId = reserva.NegocioId,
                    UsuarioId = reserva.UsuarioId,
                    Fecha = reserva.Fecha,
                    HoraInicio = reserva.HoraInicio,
                    HoraFin = reserva.HoraFin,
                    Estado = reserva.Estado,
                    FechaCreacion = reserva.FechaCreacion,
                    ServicioId = reserva.ServicioId,
                    Servicio = new ServicioDTO
                    {
                        Id = servicio.Id,
                        Nombre = servicio.Nombre
                    },
                    Pago = pago != null ? new PagoDTO
                    {
                        Id = pago.Id,
                        Monto = pago.Monto ?? 0,
                        EstadoPago = pago.EstadoPago,
                        MetodoPago = pago.MetodoPago,
                    } : null
                });
            }

            return (true, "Reservas del usuario encontradas", reservaDtos);
        }
        public async Task<(bool Success, string Message, ReservaResponseDTO? reservas)> CancelarReservaByNegocioId(int reservaId)
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

            var reservaDto = new ReservaResponseDTO
            {
                Id = reserva.Id,
                NegocioId = reserva.NegocioId,
                UsuarioId = reserva.UsuarioId,
                Fecha = reserva.Fecha,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Estado = reserva.Estado,
                FechaCreacion = reserva.FechaCreacion,
                ServicioId = reserva.ServicioId,
                Servicio = new ServicioDTO
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre
                },
                Pago = pago != null ? new PagoDTO
                {
                    Id = pago.Id,
                    Monto = pago.Monto ?? 0,
                    EstadoPago = pago.EstadoPago,
                    MetodoPago = pago.MetodoPago
                } : null
            };

            return (true, "Reserva cancelada correctamente.", reservaDto);
        }

        public async Task<(bool Success, string Message, List<ReservaResponseNegocioDTO> reservas)> GetReservaNegocioByNegocioId(int negocioId)
        {
            try
            {
                // Obtener todas las reservas del negocio con sus datos relacionados
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.Usuario,        // incluir datos usuario
                    r => r.Servicio,      // incluir datos servicio
                    r => r.Pago          // incluir pagos asociados
                );

                if (reservas == null)
                    return (true, "", new List<ReservaResponseNegocioDTO>());

                // Obtener la cantidad total de reservas por usuario en este negocio (cache para eficiencia)
                var reservasPorUsuario = reservas
                    .GroupBy(r => r.UsuarioId)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Mapear a DTO
                var resultado = reservas.Select(r =>
                {
                    var fechaFinReserva = r.Fecha.ToDateTime(r.HoraFin);
                    bool esFinalizada = DateTime.Now > fechaFinReserva;
                    

                    var reserva= new ReservaResponseNegocioDTO
                    {
                        Id = r.Id,
                        Username = r.Usuario?.UserName ?? "Usuario sin nombre",
                        NReservasUsuario = reservasPorUsuario.ContainsKey(r.UsuarioId) ? reservasPorUsuario[r.UsuarioId] : 0,
                        ServicioNombre = r.Servicio?.Nombre ?? "Servicio sin nombre",
                        ServicioDescripcion = r.Servicio?.Descripcion ?? "Sin descripcion",
                        Fecha = r.Fecha,
                        HoraInicio = TimeOnly.Parse(r.HoraInicio.ToString()),
                        HoraFin = TimeOnly.Parse(r.HoraFin.ToString()),
                        Precio = Decimal.ToDouble(r.Servicio?.Precio ?? 0),
                        Moneda = r.Pago.Moneda ?? "EUR",
                        EstadoReserva = EstadoReserva.Pendiente,
                        EstadoPago = r.Pago.EstadoPago
                    };
                    if(r.Estado == EstadoReserva.Cancelada)
                    {
                        reserva.EstadoReserva = EstadoReserva.Cancelada;
                    }
                    else
                    {
                        reserva.EstadoReserva = esFinalizada ? EstadoReserva.Finalizada : r.Estado;
                    }
                    return reserva;
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
                // Paso 1: Cargar solo las reservas del negocio específico
                var reservas = await _reservaRepo.Query()
                    .Where(r => r.NegocioId == negocioId)
                    .ToListAsync(); // Este ToList fuerza la ejecución SQL solo hasta aquí

                // Paso 2: Hacer el GroupBy en memoria
                var resultado = reservas
                    .GroupBy(r => r.Fecha.DayOfWeek)
                    .Select(g => new ReservasPorDiaDTO
                    {
                        DiaSemana = g.Key.ToString(),
                        TotalReservas = g.Count()
                    })
                    .OrderBy(r => r.DiaSemana) // opcional: orden alfabético
                    .ToList();

                return (true, "OK", resultado);
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
