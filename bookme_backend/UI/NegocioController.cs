using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Models;
using bookme_backend.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using bookme_backend.DataAcces.Repositories.Interfaces;
using System.Drawing;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.NegocioDTO;

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

                var (success, message, negocios) = await _negocioService.GetNegociosByUserId(usuarioId);

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
        [Authorize(Roles = "NEGOCIO,ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNegocio(int id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            // Obtener roles del usuario desde los claims
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            string motivoCancelacionReservas;

            // Si no se pasa motivo o está vacío, lo asignamos según el rol
    
            
            if (roles.Contains(ERol.NEGOCIO.ToString()))
            {
                motivoCancelacionReservas = "Negocio borrado, por eso se cancelan las reservas";
            }
            else if (roles.Contains(ERol.ADMIN.ToString()))
            {
                motivoCancelacionReservas = "Borrado por administrador por incumplimiento de normas de la comunidad";
            }
            else
            {
                motivoCancelacionReservas = "Borrado por motivo desconocido";
            }
            

            var (success, message) = await _negocioService.DeleteAsync(id, usuarioId, motivoCancelacionReservas);

            if (!success)
                return BadRequest(message);

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

        [HttpGet("getAll")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var (succes,message,data ) = await _negocioService.GetAllNegocios();
                if (!succes)
                    return BadRequest(new GetAllNegociosResponse
                    {
                        Success = false,
                        Message = "No se pueden los negocios",
                        InnerMessage = message, // Mensaje para detalles técnicos
                        Data = null
                    });

                return Ok(new GetAllNegociosResponse
                {
                    Success = true,
                    Message = "Negocios obtenidos correctamente",
                    InnerMessage = null,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");

                return StatusCode(500, new GetAllNegociosResponse
                {
                    Success = false,
                    Message = "Ocurrió un error inesperado. Intente más tarde.",
                    Data = null
                });
            }
        }


        [HttpGet("{id}/servicios")]
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
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
        //[Authorize(Roles = "NEGOCIO,CLIENTE")]
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
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
        [HttpPost("cliente/negocio/{id}")]
        public async Task<IActionResult> GetNegocioParaCliente(int id, [FromBody] Ubicacion? ubicacion)
        {
            var (success, message, negocio) = await _negocioService.GetNegocioParaClienteAsync(id, ubicacion);

            if (!success || negocio == null)
            {
                _logger.LogWarning($"Error al obtener negocio {id} para cliente: {message}");
                return NotFound(message);
            }

            return Ok(negocio);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/bloquear")]
        public async Task<IActionResult> BloquearNegocio(int id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var (success, message) = await _negocioService.BloquearNegocioAsync(id, usuarioId);
            if (!success)
                return BadRequest(message);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/desbloquear")]
        public async Task<IActionResult> DesbloquearNegocio(int id)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var (success, message) = await _negocioService.DesbloquearNegocioAsync(id, usuarioId);
            if (!success)
                return BadRequest(message);

            return Ok(new { Message = message });
        }
    }
}
