using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Servicio;
using bookme_backend.DataAcces.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bookme_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "NEGOCIO")]  // Solo usuarios con rol NEGOCIO pueden acceder
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _servicioService;
        private readonly ILogger<ServicioController> _logger;

        public ServicioController(IServicioService servicioService, ILogger<ServicioController> logger)
        {
            _servicioService = servicioService;
            _logger = logger;
        }

        // GET: api/Servicio
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Servicio>>> GetServicios()
        {
            var servicios = await _servicioService.GetAllServiciosAsync();
            return Ok(servicios);
        }

        // GET: api/Servicio/Negocio/5
        [Authorize(Roles = "NEGOCIO")]
        [HttpGet("Negocio/{negocioId}")]
        public async Task<ActionResult> GetServiciosByNegocioId(int negocioId)
        {
            var (success, message, servicios) = await _servicioService.GetServiciosByNegocioIdAsync(negocioId);
            if (!success)
                return NotFound(new { message });

            return Ok(servicios);
        }

        // GET: api/Servicio/Detalle/Negocio/5
        [HttpGet("Detalle/Negocio/{negocioId}")]
        public async Task<ActionResult> GetServiciosDetalleByNegocioId(int negocioId)
        {
            var (success, message, serviciosDetalle) = await _servicioService.GetServiciosDetalleByNegocioIdAsync(negocioId);
            if (!success)
                return NotFound(new { message });

            return Ok(serviciosDetalle);
        }

        // GET: api/Servicio/5
        [Authorize(Roles = "NEGOCIO")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Servicio>> GetServicio(int id)
        {
            var servicio = await _servicioService.GetServicioByIdAsync(id);
            if (servicio == null)
                return NotFound();

            return Ok(servicio);
        }
  

        [HttpGet("{id}/imagen")]
        public async Task<IActionResult> GetImagen(int id)
        {
            var (success, message, imageBytes, contentType) = await _servicioService.GetImagenByServicioIdAsync(id);

            if (!success)
                return NotFound(new { message });

            return File(imageBytes, contentType);
        }


        // POST: api/Servicio
        [HttpPost]
        [Authorize(Roles = "NEGOCIO")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> PostServicio([FromForm] ServicioDto servicio)
        {
            var (success, message, nuevoServicio) = await _servicioService.AddServicioAsync(servicio);

            if (!success)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetServicio), new { id = nuevoServicio!.Id }, nuevoServicio);
        }
        // PUT: api/Servicio/5
        [HttpPut("{id}")]
        [Authorize(Roles = "NEGOCIO")]
        public async Task<IActionResult> PutServicio(int id, ServicioUpdateDto servicio)
        {
            var (success, message, servicioActualizado) = await _servicioService.UpdateServicioAsync(id, servicio);
            if (!success)
                return BadRequest(new { message });

            return Ok(servicioActualizado);
        }

        [HttpPut("{id}/imagen")]
        [Authorize(Roles = "NEGOCIO")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutImagenServicio(int id, [FromForm] Imagen imagen)
        {
            var (success, message) = await _servicioService.UpdateImagenServicioAsync(id, imagen);
            if (!success)
                return BadRequest(new { message });

            return NoContent();
        }


        // DELETE: api/Servicio/5
        [Authorize(Roles = "NEGOCIO")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicio(int id)
        {
            var (success, message) = await _servicioService.DeleteServicioAsync(id);
            if (!success)
                return NotFound(new { message });

            return NoContent();
        }
        // GET: api/Servicio/Detalle
        [HttpGet("Detalle")]
        [Authorize(Roles = "NEGOCIO,CLIENTE")]
        public async Task<ActionResult> GetServiciosDetalle()
        {
            var (success, message, serviciosDetalle) = await _servicioService.GetServiciosDetalleAsync();

            if (!success)
                return NotFound(new { message });

            return Ok(serviciosDetalle);
        }
    }
}
