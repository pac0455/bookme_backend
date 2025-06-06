using Azure.Core;
using bookme_backend.BLL.Exceptions;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Usuario;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using bookme_backend.UI.Controllers;
using Humanizer;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace bookme_backend.BLL.Services
{
    public class UsuarioService(
        IUsuarioRepository usuarioRepository,
        IConfiguration configuration,
        UserManager<Usuario> userManager,
        ICustomEmailSender emailSender,
        ILogger<UsuarioService> logger,
        RoleManager<IdentityRole> roleManager
    ) : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly ICustomEmailSender _emailSender = emailSender;
        private readonly ILogger<UsuarioService> _logger = logger;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;




        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailAsync(email);
        }



        public async Task<LoginResultDTO> Login(string email, string password)
        {
            var errores = await ValidarErroresLoginAsync(email, password);
            if (errores.Any())
                throw new ValidationException("Errores de validación en el login", errores);

            var user = await _userManager.FindByEmailAsync(email); // ya está validado que existe
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user);

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
        public async Task<LoginResultDTO> RegisterAsync(RegisterDTO model)
        {
            var errores = await ValidarErroresRegistroAsync(model);
            if (errores.Any())
                throw new ValidationException("Errores de validación en el registro", errores);

            var user = new Usuario
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // Esto ya no debería ocurrir si ValidarErroresRegistroAsync valida correctamente.
                // Pero puedes dejarlo por seguridad como fallback.
                foreach (var error in result.Errors)
                {
                    errores["password"] = errores.ContainsKey("password")
                        ? $"{errores["password"]} {error.Description}"
                        : error.Description;
                }
                throw new ValidationException("Errores de validación al crear el usuario", errores);
            }

            var rol = model.IsNegocio ? ERol.NEGOCIO.ToString() : ERol.CLIENTE.ToString();
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

        public async Task<Dictionary<string, string>> ValidarErroresRegistroAsync(RegisterDTO model)
        {
            var errores = new Dictionary<string, string>();

            // Validar nombre de usuario
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                errores["username"] = "El nombre de usuario no puede estar vacío.";
            }
            else
            {
                if (!model.UserName.All(char.IsLetterOrDigit))
                {
                    errores["username"] = "El nombre de usuario solo puede contener letras y números.";
                }

                var usernameExists = await _usuarioRepository.Exist(user =>
                    user.NormalizedUserName == model.UserName.ToUpperInvariant());

                if (usernameExists)
                {
                    errores["username"] = "El nombre de usuario ya está en uso.";
                }
            }

            // Validar email
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                errores["email"] = "El correo electrónico no puede estar vacío.";
            }
            else
            {
                if (!model.Email.Contains("@"))
                {
                    errores["email"] = "El correo electrónico no es válido.";
                }

                var emailExists = await _usuarioRepository.Exist(user =>
                    user.NormalizedEmail == model.Email.ToUpperInvariant());

                if (emailExists)
                {
                    errores["email"] = "El correo electrónico ya está registrado.";
                }
            }

            // Validar número de teléfono
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                errores["phoneNumber"] = "El número de teléfono no puede estar vacío.";
            }
            else if (!model.PhoneNumber.All(char.IsDigit) || model.PhoneNumber.Length != 9)
            {
                errores["phoneNumber"] = "El número de teléfono debe contener 9 dígitos.";
            }
            else
            {
                var phoneExists = await _usuarioRepository.Exist(user =>
                    user.PhoneNumber == model.PhoneNumber);

                if (phoneExists)
                {
                    errores["phoneNumber"] = "El número de teléfono ya está en uso.";
                }
            }

            // Validar contraseña con los validadores del UserManager
            var fakeUser = new Usuario { UserName = model.UserName, Email = model.Email };
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, fakeUser, model.Password);
                if (!result.Succeeded)
                {
                    errores["password"] = string.Join(" ", result.Errors.Select(e => e.Description));
                    break;
                }
            }

            return errores;
        }


        public async Task<Dictionary<string, string>> ValidarErroresLoginAsync(string email, string password)
        {
            var errores = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(email))
                errores["email"] = "El correo electrónico no puede estar vacío.";

            if (string.IsNullOrWhiteSpace(password))
                errores["password"] = "La contraseña no puede estar vacía.";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                errores["email"] = "No existe un usuario con ese correo.";
                return errores;
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                errores["password"] = "La contraseña es incorrecta.";

            return errores;
        }

        public Task<bool> UsuarioTieneServiciosAsync(int usuarioId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, List<ClienteResumenDTO> Data)> GetEstadisticasUsuariosPorNegocioAsync(int negocioId)
        {
            throw new NotImplementedException();
        }

        Task<(bool Success, string Message, List<ClienteResumenDTO> Data)> IUsuarioService.GetEstadisticasUsuariosPorNegocioAsync(int negocioId)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, UpdateNombreDTO usuarioActualizado, string Message)> UpdateUsuarioNombreAsync(UpdateNombreDTO newUser)
        {

            var usuario = await _userManager.FindByIdAsync(newUser.Id);

            //Validar que el usuario existe
            if (usuario == null)
            {
                return (false, newUser, "Usuario no encontrado.");
            }
            //Actualizar el nombre de usuario
            usuario.UserName = newUser.UserName;
            usuario.NormalizedUserName = newUser.UserName.ToUpper();

            //Guardar
            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                return (false, newUser, $"Error al actualizar el usuario: {errores}");
            }
            // Actualizar la respuesta
            newUser.UserName = usuario.UserName;
            
            return (true, newUser, "");
        }
        public async Task<(bool Success, string Message)> UpdatePasswordAsync(UpdatePasswordDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, errorMessages);
            }

            return (true, "Contraseña actualizada correctamente.");
        }

    }
}
