using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using bookme_backend.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using bookme_backend.DataAcces.Repositories.Interfaces;
using System.Drawing;
using bookme_backend.DataAcces.DTO;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class NegocioController : ControllerBase
    {
        private readonly BookmeContext _context;
        private readonly ILogger<NegocioController> _logger;
        private readonly INegocioService _negocioService;
        private readonly IRepository<Negocio> _repository;
        public NegocioController(BookmeContext context, INegocioService negocioService, ILogger<NegocioController> logger, IRepository<Negocio> repository)
        {
            _context = context;
            _negocioService = negocioService;
            _logger = logger;
            _repository = repository;
        }
 


        // GET: api/Negocio
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Negocio>>> GetNegocios()
        {
            return await _context.Negocios.Include(x => x.HorariosAtencion).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Negocio>> GetNegocio(int id)
        {
            var negocio = await _context.Negocios
                .Include(n => n.HorariosAtencion)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (negocio == null)
            {
                return NotFound();
            }

            return negocio;
        }
        [Authorize(Roles = "NEGOCIO")]
        [HttpGet("ByUserId")]
        public async Task<ActionResult<IEnumerable<Negocio>>> GetNegiciosByUserID()
        {
            try
            {
                _logger.LogInformation("Entrando a GetNegiciosByUserID");

                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(usuarioId))
                {
                    _logger.LogWarning("User ID not found in token.");
                    return Unauthorized("No se pudo obtener el ID del usuario.");
                }

                var (success, message, negocios) = await _negocioService.GetByUserId(usuarioId);

                if (!success)
                {
                    _logger.LogError($"Error al obtener negocios por usuario: {message}");
                    return BadRequest(message);
                }

                return negocios;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al obtener negocios: {ex.Message}");
                return BadRequest($"Ocurrió un error: {ex.Message}");
            }
        }

        [Authorize(Roles = "NEGOCIO")]
        [HttpPut("{id}/imagen")]
        public async Task<IActionResult> UpdateNegocioImagen(int id, Imagen nuevaImagen)
        {
            if (nuevaImagen == null)
                return BadRequest("Debe enviar una imagen.");

            var (success, message, negocio) = await _negocioService.UpdateNegocioImagenAsync(id, nuevaImagen);

            if (!success)
                return BadRequest(message);

            return Ok(negocio);
        }


        [HttpGet("{id}/imagen")]
        public async Task<IActionResult> GetNegocioImagen(int id)
        {
            var (success, message, imageBytes, contentType) = await _negocioService.GetNegocioImagenAsync(id);

            if (!success || imageBytes == null)
            {
                _logger.LogWarning($"Error al obtener imagen del negocio {id}: {message}");
                return NotFound(message);
            }

            return File(imageBytes, contentType);
        }
        [Authorize(Roles = "NEGOCIO")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNegocio(int id, Negocio negocio)
        {
            if (id != negocio.Id)
                return BadRequest("ID del negocio no coincide.");

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var (success, message) = await _negocioService.UpdateAsync(negocio, usuarioId);

            if (!success)
                return BadRequest(message);

            return NoContent();
        }
        [Authorize(Roles = "NEGOCIO")]
        [HttpPut("ByNombre/{nombre}")]
        public async Task<IActionResult> PutNegocioPorNombre(string nombre, [FromBody] Negocio negocio)
        {
            if (!string.Equals(nombre, negocio.Nombre, StringComparison.OrdinalIgnoreCase))
                return BadRequest("El nombre en la URL no coincide con el del negocio.");

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var (success, message) = await _negocioService.UpdateByNombreAsync(negocio, usuarioId);

            if (!success)
                return BadRequest(message);

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
        [Authorize(Roles = "NEGOCIO")]
        [HttpGet("{id}/reservas")]
        public async Task<IActionResult> GetReservasByNegocioId(int id)
        {
            var (success, message, reservas) = await _negocioService.GetReservasByNegocioIdAsync(id);

            if (!success)
            {
                _logger.LogError($"Error al obtener reservas del negocio {id}: {message}");
                return BadRequest(message);
            }

            return Ok(reservas);
        }

        [HttpGet("{id}/reservas/detalladas")]
        public async Task<IActionResult> GetReservasDetalladas(int id)
        {
            var (success, message, reservas) = await _negocioService.GetReservasDetalladasByNegocioIdAsync(id);

            if (!success)
            {
                _logger.LogError($"Error al obtener reservas detalladas del negocio {id}: {message}");
                return BadRequest(message);
            }

            return Ok(reservas);
        }


        [HttpGet("{id}/servicios")]
        public async Task<IActionResult> GetServiciosByNegocioId(int id)
        {
            var (success, message, servicios) = await _negocioService.GetServiciosByNegocioIdAsync(id);

            if (!success)
            {
                _logger.LogError($"Error al obtener servicios del negocio {id}: {message}");
                return BadRequest(message);
            }

            return Ok(servicios);
        }
        [HttpPost("cliente/negocios")]
        public async Task<IActionResult> GetNegociosParaClienteAsync(Ubicacion? ubicacion)
        {
            var (success, message, negocios) = await _negocioService.GetNegociosParaClienteAsync(ubicacion);

            if (!success)
            {
                _logger.LogError($"Error al obtener los negocios: {message}");
                return BadRequest(message);
            }

            return Ok(negocios);
        }
    }
}
