
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using bookme_backend.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class NegocioController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly ILogger<NegocioController> _logger;
        private readonly INegocioService _negocioService;

        public NegocioController(BookmeContext context, INegocioService negocioService, ILogger<NegocioController> logger)
        {
            _context = context;
            _negocioService = negocioService;
            _logger = logger;
        }
        [Authorize(Roles = "NEGOCIO")]
        // GET: api/Negocio
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Negocio>>> GetNegocios()
        {
            return await _context.Negocios.Include(x => x.HorariosAtencion).ToListAsync();
        }
        [Authorize(Roles = "NEGOCIO")]
        // GET: api/Negocio/5
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
        [Authorize(Roles = "NEGOCIO")]
        // PUT: api/Negocios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNegocio(int id, Negocio negocio)
        {
            // Check if the ID in the URL matches the ID in the negocio object
            if (id != negocio.Id)
            {
                return BadRequest();
            }

            // Mark the negocio entity as modified
            _context.Entry(negocio).State = EntityState.Modified;

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency issues
                if (!NegocioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Rethrow the exception if it's not a concurrency issue
                }
            }

            // Return a NoContent response if the update is successful
            return NoContent();
        }

        // POST: api/Negocios
        [HttpPost]
        [Authorize(Roles = "NEGOCIO")]
        public async Task<ActionResult<Negocio>> PostNegocio(Negocio negocio)
        {
            try
            {
                _logger.LogInformation("Entering PostNegocio method.");

                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Su id es: {usuarioId}");

                if (string.IsNullOrEmpty(usuarioId))
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized("No se pudo obtener el ID del usuario.");
                }

                // No necesitas chequear rol aquí porque el atributo Authorize ya lo hizo

                var (success, message) = await _negocioService.AddAsync(negocio, usuarioId);

                if (!success)
                {
                    _logger.LogError($"Failed to add negocio: {message}");
                    return BadRequest(message);
                }

                _logger.LogInformation("Negocio created successfully.");
                return CreatedAtAction("GetNegocio", new { id = negocio.Id }, negocio);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }
        }

        [Authorize(Roles = "NEGOCIO")]
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
