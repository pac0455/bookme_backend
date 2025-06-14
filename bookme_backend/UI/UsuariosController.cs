using Microsoft.AspNetCore.Mvc;
using bookme_backend.DataAcces.DTO;
using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Exceptions;
using bookme_backend.DataAcces.DTO.Usuario;
using Microsoft.AspNetCore.Authorization;
using bookme_backend.BLL.Services;

namespace bookme_backend.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ICustomEmailSender _customEmailSender;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger,
            ICustomEmailSender customEmailSender,
            IUsuarioService usuarioService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            ICustomEmailSender _customEmailSender;
        }

        [HttpPost("validar-registro")]
        public async Task<IActionResult> ValidarRegistro([FromBody] RegisterDTO model)
        {
            try
            {
                var errores = await _usuarioService.ValidarErroresRegistroAsync(model);
                if (errores.Any())
                    return Ok(new { Success = false, errores });

                return Ok(new { Success = true, errores });
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


        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _usuarioService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, ex.Message);
            }
        }

        //[Authorize(Roles = "CLIIENTE,NEGOCIO,ADMIN")]
        [HttpGet("sendMail/{id}")]
        public async Task<IActionResult> SendMail(string id)
        {
            try
            {
                var response = await _usuarioService.SendAuthenticationCodeAsync(id);
                if(!response.Success)
                {
                    return BadRequest(new { message = response.Message });
                }
                return Ok(new { message = response.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario");
                return StatusCode(500, new { message = "Error al obtener el usuario" });
            }
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmEmail(ConfirmMailDTO model)
        {
            try
            {
                var (Success, Message) = await _usuarioService.VerifyCodeAsync(
                    code: model.CODE,
                    userId: model.Id
                );


                if(!Success)
                {
                    return BadRequest(new { message = Message });
                }
                _logger.LogInformation("Email confirmado exitosamente");
                return Ok(new { message = Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar el email");
                return StatusCode(500, new { message = "Error al confirmar el email" });
            }
        }

        [Authorize(Roles = "CLIIENTE,NEGOCIO,ADMIN")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] string id)
        {
            try
            {
                var (success, message) = await _usuarioService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { message });

                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario");
                return StatusCode(500, new { message = "Error al eliminar el usuario" });
            }
        }

        [HttpPut("update-nombre")]
        public async Task<IActionResult> UpdateUsuarioNombre([FromBody] UpdateNombreDTO model)
        {
            try
            {
                var (succes,usuarioActualizado, message) = await _usuarioService.UpdateUsuarioNombreAsync(model);
                if(!succes)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { usuarioActualizado, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el nombre del usuario");
                return StatusCode(500, new { message = "Error interno del servidor." });
            }
        }
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO model)
        {
            var result = await _usuarioService.UpdatePasswordAsync(model);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
        // POST api/usuarios/{id}/bloquear
        [HttpPut("{id}/bloquear")]
        public async Task<IActionResult> BloquearUsuario(string id)
        {
            var (Success, Message) = await _usuarioService.BloquearUsuarioAsync(id);
            if (!Success)
                return BadRequest(new { success = false, message = Message });

            return Ok(new { success = true, message = Message });
        }

        // POST api/usuarios/{id}/desbloquear
        [HttpPut("{id}/desbloquear")]
        public async Task<IActionResult> DesbloquearUsuario(string id)
        {
            var (Success, Message) = await _usuarioService.DesbloquearUsuarioAsync(id);
            if (!Success)
                return BadRequest(new { success = false, message = Message });

            return Ok(new { success = true, message = Message });
        }
    }
}