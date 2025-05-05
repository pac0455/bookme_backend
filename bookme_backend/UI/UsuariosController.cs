using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.DTO;
using Microsoft.AspNetCore.Identity.UI.Services;
using bookme_backend.BLL.Services;
using Org.BouncyCastle.Utilities;
using bookme_backend.BLL.Interfaces; // Cambia por el namespace correcto si tu modelo Usuario está en otro sitio

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


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            try
            {
                var result = await _usuarioService.RegisterAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                return StatusCode(500, ex.Message);
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
            }catch(Exception ex)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var (success, message) = await _usuarioService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(message);
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario");
                return StatusCode(500, "Error interno del servidor.");
            }
        }


    }
}
