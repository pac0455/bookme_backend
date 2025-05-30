﻿using Microsoft.AspNetCore.Mvc;
using bookme_backend.DataAcces.DTO;
using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Exceptions;

namespace bookme_backend.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger, IUsuarioService usuarioService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
        }

        [HttpPost("validar-registro")]
        public async Task<IActionResult> ValidarRegistro([FromBody] RegisterDTO model)
        {
            try
            {
                var errores = await _usuarioService.ValidarErroresRegistroAsync(model);
                if(errores.Any())
                    return Ok(new { Success = false, errores });

                return Ok(new { Success = true,  errores });
            }
          
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ValidarRegistro");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor." });
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                var result = await _usuarioService.RegisterAsync(model);
                return Ok(new { success = true, data = result });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = vex.Message,       // Mensaje general
                    errores = vex.Errores        // Diccionario de errores por campo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor.",
                    details = ex.Message
                });
            }
        }
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Llamamos al servicio para manejar toda la lógica
            var (success, message) = await _usuarioService.ResendConfirmationEmailAsync(model.Email);

            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            try
            {
                var result = await _usuarioService.Login(model.Email, model.Password);
                return Ok(result);
            }
            catch (ValidationException vex)
            {
                return BadRequest(new
                {
                    Success = false,
                    vex.Message,
                    vex.Errores
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var (success, message) = await _usuarioService.ForgotPasswordAsync(model.Email, baseUrl);

            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _usuarioService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try{
                var users = await _usuarioService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("estadisticas-usuarios/{negocioId}")]
        public async Task<IActionResult> GetEstadisticasUsuariosPorNegocio(int negocioId)
        {
            try
            {
                var (success, message, data) = await _usuarioService.GetEstadisticasUsuariosPorNegocioAsync(negocioId);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new
                {
                    Success = true,
                    Message = message,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener estadísticas de usuarios para negocio {negocioId}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Error interno al procesar la solicitud",
                    Data = new List<UsuarioReservaEstadisticaDto>()
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery]string email)
        {
            try
            {
                var (success, message) = await _usuarioService.DeleteAsync(email);
                if (!success)
                {
                    return BadRequest(new {message});

                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario" );
                return StatusCode(500, new { message = "Error al eliminar el usuario" });
            }
        }
    }
}
