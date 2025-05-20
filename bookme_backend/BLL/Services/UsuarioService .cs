using Azure.Core;
using bookme_backend.BLL.Exceptions;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using bookme_backend.UI.Controllers;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace bookme_backend.BLL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Usuario> _userManager;
        private readonly ICustomEmailSender _emailSender;
        private readonly ILogger<UsuarioService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IPasswordHelper passwordHelper, 
            IConfiguration configuration,
            UserManager<Usuario> userManager,
            ICustomEmailSender emailSender,
            ILogger<UsuarioService> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHelper = passwordHelper;
            _configuration = configuration;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _roleManager = roleManager;
        }
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailAsync(email);
        }
        public async Task<LoginResultDTO> Login(string email, string password)
        {
            // 1. Buscar el usuario desde Identity (no desde tu repositorio personalizado)
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado.");

            // 2. Verificar la contraseña con UserManager (recomendado)
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Contraseña incorrecta.");

            // 3. Obtener los roles correctamente
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Generar el token
            var token = await GenerateJwtToken(user);

            // 5. Devolver DTO
            return new LoginResultDTO
            {
                Usuario = user,
                Token = token,
                Roles = roles
            };
        }

        private async Task<string> GenerateJwtToken(Usuario user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Agrega cada rol como ClaimTypes.Role
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

        public async Task<(bool Success, string Message)> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "No existe un usuario con este email.");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return (false, "El email ya ha sido confirmado.");

            try
            {
                // Generar el token de confirmación
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Construir el enlace de confirmación
                var confirmationLink = $"myapp://confirmemail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Enviar el correo electrónico
                await _emailSender.SendEmailAsync(email, "Confirmación de correo electrónico",
                    $"Por favor, confirma tu correo haciendo clic en el siguiente enlace: <a href='{confirmationLink}'>Confirmar correo</a>.");

                return (true, "El correo de confirmación ha sido reenviado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar el correo de confirmación.");
                return (false, "Error al reenviar el correo de confirmación.");
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string resetPasswordBaseUrl)
        {
            var user = await _userManager.FindByEmailAsync(email.ToUpperInvariant());
            if (user == null)
                return (false, "No existe un usuario con ese correo.");


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Generar el enlace de restablecimiento de contraseña como un Deep Link
            var resetPasswordLink = $"{resetPasswordBaseUrl}/resetpassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            try
            {
                await _emailSender.SendEmailAsync(email, "Restablecer contraseña",
                    $"Por favor, restablezca su contraseña haciendo clic aquí: <a href='{resetPasswordLink}'>enlace</a>.");

                return (true, "Se ha enviado un enlace para restablecer la contraseña.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo de restablecimiento de contraseña");
                return (false, "Error al enviar el correo de restablecimiento de contraseña.");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Usuario no encontrado.");

            // Decodificar el token
            var decodedToken = Uri.UnescapeDataString(token);

            // Intentar restablecer la contraseña
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Error al restablecer la contraseña: {errors}");
                return (false, errors);
            }

            return (true, "La contraseña se ha restablecido correctamente.");
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }

        public async Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid)
        {
            return await _usuarioRepository.GetByFirebaseUidAsync(uid);
        }

       
        public async Task<Usuario?> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new KeyNotFoundException($"No se encontró el usuario con ID {id}");

            return usuario;
        }

        public async Task SaveChangesAsync()
        {
            await _usuarioRepository.SaveChangesAsync();
        }

        public async Task Update(Usuario usuario)
        {
            _usuarioRepository.Update(usuario);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email.ToUpperInvariant());

            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var resultado = await _userManager.DeleteAsync(usuario);

            if (resultado.Succeeded)
            {
                return (true, "Usuario eliminado correctamente.");
            }

            var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
            return (false, $"Error al eliminar el usuario: {errores}");
        }




        //Devuelve la misma respuesta que el login
        public async Task<LoginResultDTO> RegisterAsync(RegisterDTO model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email.ToUpperInvariant());
            if (existingUser != null)
                throw new Exception("El correo electrónico ya está registrado");

            var user = new Usuario
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));


            // Determinar el rol
            var rol = model.IsNegocio ? ERol.NEGOCIO.ToString() : ERol.CLIENTE.ToString();

            if (string.IsNullOrWhiteSpace(rol))
                throw new InvalidOperationException("El rol no puede ser nulo o vacío.");
            // Crear el rol si no existe
            if (!await _roleManager.RoleExistsAsync(rol))
                await _roleManager.CreateAsync(new IdentityRole(rol));

            await _userManager.AddToRoleAsync(user, rol);
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user);


            return new LoginResultDTO
            {
                Usuario = user,
                Token = token,
                Roles = roles
            };
        }
    }
}
