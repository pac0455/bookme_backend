using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Services;
using bookme_backend.DataAcces.DTO.Valoraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValoracionesController : ControllerBase
    {
        private readonly IValoracionesService _valoracionesService;
        public ValoracionesController(IValoracionesService valoracionesService)
        {
            _valoracionesService = valoracionesService;
        }
        // GET: api/Valoraciones/Negocio/5
        [HttpGet("Negocio/{negocioId}")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> GetValoracionesByNegocioId(int negocioId)
        {
            var (success, message, valoraciones) = await _valoracionesService.GetValoracionesByNegocioId(negocioId);
            if (!success)
                return NotFound(new { message });
            return Ok(valoraciones);
        }
        // POST: api/Valoraciones
        [HttpPost]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> CreateValoracion([FromBody] ValoracionCreateDTO valoracionDto)
        {
            var (success, message, valoracion) = await _valoracionesService.Create(valoracionDto);
            if (!success)
                return BadRequest(new { message });
            return CreatedAtAction(nameof(GetValoracionesByNegocioId), new { negocioId = valoracion.NegocioId }, valoracion);
        }
    }
}
