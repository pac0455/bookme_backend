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
           

            // Llamamos al método actualizado que ya no espera lista de servicios
            var (success, message, reservaCreada) = await _reservaService.CrearReservaAsync(dto);

            if (!success || reservaCreada == null)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetReserva), new { id = reservaCreada.Id }, reservaCreada);
        }

        [Authorize(Roles = "CLIENTE")]
        [HttpGet("Usuario/{userId}/Todas")]
        public async Task<IActionResult> GetReservasByUserId(string userId)
        {
            var (success, message, reservas) = await _reservaService.GetReservasByUserIdAsync(userId);

            if (!success || reservas == null)
                return NotFound(new { message });
            return Ok(reservas);
        }



        [HttpPut("Cancelar/{id}")]
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
        public async Task<IActionResult> CancelarReservaPorNegocio(int id)
        {
            var (success, message, reservaCancelada) = await _reservaService.CancelarReservaByNegocioId(id);

            if (!success)
                return BadRequest(new { message });

            return Ok( reservaCancelada);
        }
    }
}
