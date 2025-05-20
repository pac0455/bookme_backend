using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using bookme_backend.BLL.Services;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class NegocioController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly NegocioService _negocioService;

        public NegocioController(BookmeContext context)
        {
            _context = context;
        }

        // GET: api/Negocios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Negocio>>> GetNegocios()
        {
            return await _context.Negocios.ToListAsync();
        }

        // GET: api/Negocios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Negocio>> GetNegocio(int id)
        {
            var negocio = await _context.Negocios.FindAsync(id);

            if (negocio == null)
            {
                return NotFound();
            }

            return negocio;
        }

        // PUT: api/Negocios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNegocio(int id, Negocio negocio)
        {
            if (id != negocio.Id)
            {
                return BadRequest();
            }

            _context.Entry(negocio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NegocioExists(id))
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

        // POST: api/Negocios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Negocio>> PostNegocio(Negocio negocio)
        {
            try
            {
                var (succes, message) =await _negocioService.AddAsync(negocio);
                if(!succes )  return BadRequest(message);
                return CreatedAtAction("GetNegocio", new { id = negocio.Id }, negocio);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }            
        }

        // DELETE: api/Negocios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNegocio(int id)
        {
            var negocio = await _context.Negocios.FindAsync(id);
            if (negocio == null)
            {
                return NotFound();
            }

            _context.Negocios.Remove(negocio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NegocioExists(int id)
        {
            return _context.Negocios.Any(e => e.Id == id);
        }
    }
}
