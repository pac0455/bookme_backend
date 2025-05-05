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
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioService _usuarioService;

        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ICustomEmailSender _emailSender;
        private readonly ILogger<UsuarioController> _logger;




        public UsuarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IConfiguration configuration,
            ICustomEmailSender emailSender,
            ILogger<UsuarioController> logger,
            IUsuarioService usuarioService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger;
            _usuarioService = usuarioService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            // Validar el modelo
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si el email ya está registrado
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("El correo electrónico ya está registrado");

            var user = new Usuario
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            try
            {
                // Generar token de confirmación
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, token = confirmationToken },
                    Request.Scheme);

                // Usar el método específico para confirmación de email
                await _emailSender.SendConfirmationLinkAsync(
                    user,
                    user.Email,
                    confirmationLink);

                return Ok(new
                {
                    message = "Usuario registrado correctamente. Por favor revisa tu email para confirmar tu cuenta.",
                    userId = user.Id // Opcional: devolver el ID para referencia
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de confirmación");

                // Opción 1: Borrar el usuario si falla el envío del email
                await _userManager.DeleteAsync(user);

                // Opción 2: Devolver éxito pero indicar que debe solicitar reenvío
                // return Ok(new { 
                //     message = "Usuario registrado pero falló el envío de confirmación",
                //     userId = user.Id
                // });

                return StatusCode(500, "Error al enviar el email de confirmación");
            }
        }
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("No existe un usuario con este email");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest("El email ya ha sido confirmado");

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, token },
                    Request.Scheme
                );

                await _emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

                return Ok(new
                {
                    message = "Email de confirmación reenviado",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar email de confirmación");
                return StatusCode(500, "Error al reenviar el email de confirmación");
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Credenciales incorrectas." });
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Credenciales incorrectas." });

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }
        private string GenerateJwtToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("No existe un usuario con ese correo.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 🔥 Normalmente aquí generas un enlace para que haga reset
            var callbackUrl = $"https://tu-frontend/resetpassword?email={model.Email}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(model.Email, "Restablecer contraseña",
                $"Por favor, restablezca su contraseña haciendo clic aquí: <a href='{callbackUrl}'>enlace</a>.");

            return Ok("Se ha enviado un enlace para restablecer la contraseña.");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Usuario no encontrado.");
            var decodedToken = Uri.UnescapeDataString(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("La contraseña se ha restablecido correctamente.");
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
    }
}
