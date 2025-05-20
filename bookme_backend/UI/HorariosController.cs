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

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorariosController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly HorarioService _horarioService;

        public HorariosController(BookmeContext context, IRepository<Horarios> repository)
        {
            _context = context;
            _repository = repository;
        }



        // GET: api/Horarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Horarios>>> GetHorarios()
        {
            return await _repository.GetAllAsync();
        }

        // GET: api/Horarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Horarios>> GetHorarios(int id)
        {
            var horarios = await _context.Horarios.FindAsync(id);

            if (horarios == null)
            {
                return NotFound();
            }

            return horarios;
        }

        // PUT: api/Horarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorarios(int id, Horarios horarios)
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
                if (!HorariosExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Horarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Horarios>> PostHorarios(Horarios horarios)
        {
            _context.Horarios.Add(horarios);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHorarios", new { id = horarios.Id }, horarios);
        }

        [HttpPost("/range")]
        public async Task<ActionResult<List<Horarios>>> PostRangeHorarios(List<Horarios> horarios)
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

        private bool HorariosExists(int id)
        {
            return _context.Horarios.Any(e => e.Id == id);
        }
    }
}
