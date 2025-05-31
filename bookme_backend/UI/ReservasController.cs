using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using Microsoft.AspNetCore.Authorization;
using bookme_backend.DataAcces.Repositories.Interfaces;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly IReservaService _reservaService;
        private readonly IHorarioService _horarioService;



        public ReservasController(BookmeContext context, IReservaService reservaService, IHorarioService horarioService)
        {
            _context = context;
            _horarioService = horarioService;
            _reservaService = reservaService;
        }

        // GET: api/Horarios/Disponibles
        [HttpGet("Disponibles")]
        public async Task<IActionResult> GetHorarioDisponible(int negocioId, int servicioId, DateOnly date)
        {
            // Llamamos al método del servicio
            var (success, message, horarios) = await _horarioService.GetHorarioServicioSinReservaByNegocioID(negocioId, servicioId, date);

            if (!success)
            {
                // Si no fue exitoso, devolvemos un mensaje de error
                return BadRequest(new { message });
            }

            // Si fue exitoso, devolvemos los horarios disponibles
            return Ok(horarios);
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return reserva;
        }

        // PUT: api/Reservas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return BadRequest();
            }

            _context.Entry(reserva).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST: api/Reservas
        [Authorize(Roles = "CLIENTE")]
        [HttpPost]
        public async Task<IActionResult> PostReserva(ReservaCreateDTO dto)
        {
            // Mapeamos DTO a modelo Reserva
            var reserva = new Reserva
            {
                NegocioId = dto.NegocioId,
                UsuarioId = dto.UsuarioId,
                Fecha = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Estado = dto.Estado,
                ComentarioCliente = dto.ComentarioCliente
            };

            // Crear el objeto Pago solo si se requiere
            Pago? pago = null;
            if (dto.Pago != null)
            {
                // Asumimos que el pago se realiza en físico
                pago = new Pago
                {
                    Monto = dto.Pago.Monto,
                    EstadoPago = EstadoPago.Confirmado, // Asumimos que el pago se confirma al momento
                    MetodoPago = "Efectivo", // O el método que desees usar para pagos en físico
                    Creado = DateTime.UtcNow
                };
            }

            var (success, message, reservaCreada) = await _reservaService.CrearReservaAsync(reserva, dto.ServicioIds, pago);

            if (!success || reservaCreada == null)
                return BadRequest(new { message });

            // Cargar explícitamente relaciones si quieres, o devolver reservaCreada directamente si ya está cargada
            return CreatedAtAction(nameof(GetReserva), new { id = reservaCreada.Id }, reservaCreada);
        }



        // DELETE: api/Reservas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
