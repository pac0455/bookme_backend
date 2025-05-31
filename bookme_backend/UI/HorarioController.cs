using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using bookme_backend.BLL.Services;
using bookme_backend.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorarioController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly IHorarioService _horarioService;


        public HorarioController(BookmeContext context, IHorarioService horarioService)
        {
            _context = context;
            _horarioService = horarioService;
        }



        // GET: api/Horarios/Disponibles/5/3/2025-06-01
        //[Authorize(Roles = "CLIENTE,NEGOCIO")]
        [HttpGet("Disponibles/{negocioId:int}/{servicioId:int}/fecha")]
        public async Task<ActionResult<List<Horario>>> GetHorariosDisponibles(
            int negocioId,
            int servicioId,
            String fecha)
        {
            try
            {
                if (negocioId <= 0 || servicioId <= 0)
                {
                    return BadRequest("Los IDs deben ser mayores que cero.");
                }
                if (!DateTime.TryParse(fecha, out var fechaParseada))
                    return BadRequest("Fecha inválida.");


                var dateOnly = DateOnly.FromDateTime(fechaParseada);

                var (success, message, horarios) =
                    await _horarioService.GetHorarioServicioSinReservaByNegocioID(negocioId, servicioId, dateOnly);

                if (!success)
                {
                    return BadRequest(message);
                }

                return Ok(horarios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET: api/Horarios/5
        [Authorize(Roles = "CLIENTE,NEGOCIO")]
        [HttpGet("ByNegocioId/{id}")]
        public async Task<ActionResult<Horario>> GetHorariosByNegocioId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("El ID del negocio debe ser mayor que cero.");
                }
                var (result, message, horarios) = await _horarioService.GetHorariosByNegocioId(id);
                if (!result)
                {
                    return BadRequest(message);
                }
                return Ok(horarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // PUT: api/Horarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorarios(int id, Horario horarios)
        {
            if (id != horarios.Id)
            {
                return BadRequest();
            }

            _context.Entry(horarios).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
            }

            return NoContent();
        }

        // POST: api/Horarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Horario>> PostHorarios(Horario horarios)
        {
            _context.Horarios.Add(horarios);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHorarios", new { id = horarios.Id }, horarios);
        }

        [HttpPost("/range")]
        public async Task<ActionResult<List<Horario>>> PostRangeHorarios(List<Horario> horarios)
        {
            try
            {
                var (succes, message) = await _horarioService.AddRangeAsync(horarios);
                if(!succes) return BadRequest(message);
                return Ok(horarios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // DELETE: api/Horarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorarios(int id)
        {
            var horarios = await _context.Horarios.FindAsync(id);
            if (horarios == null)
            {
                return NotFound();
            }

            _context.Horarios.Remove(horarios);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
