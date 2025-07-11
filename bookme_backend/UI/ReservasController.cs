﻿using Microsoft.AspNetCore.Mvc;
using bookme_backend.DataAcces.Models;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("negocio/{negocioId}/reservas")]
        public async Task<IActionResult> GetReservasPorNegocio(int negocioId)
        {
            var (success, message, reservas) = await _reservaService.GetReservaNegocioByNegocioId(negocioId);

            if (!success)
                return NotFound(new { Message = message });

            return Ok(reservas);
        }


        // GET: api/Horarios/Disponibles
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
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
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
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
        // GET: api/Reservas/Estadisticas/PorDiaSemana
        [HttpGet("Estadisticas/PorDiaSemana")]
        [Authorize(Roles = "NEGOCIO,ADMINISTRADOR")]
        public async Task<IActionResult> GetReservasPorDiaSemana([FromQuery] int negocioId)
        {
            var (success, message, data) = await _reservaService.GetReservasPorDiaSemanaAsync(negocioId);

            if (!success)
                return BadRequest(new { message });

            return Ok(data);
        }


        [Authorize(Roles = "NEGOCIO,ADMINISTRADOR")]
        [HttpPut("ActualizarEstadoPago/{reservaId}")]
        public async Task<IActionResult> CambiarEstadoPago(int reservaId, [FromQuery] EstadoPago nuevoEstado)
        {
            var (success, message) = await _reservaService.CambiarEstadoPagoDeReservaAsync(reservaId, nuevoEstado);

            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
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
