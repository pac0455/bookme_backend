using Azure.Core;
using bookme_backend.BLL.Exceptions;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
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
        IPasswordHelper passwordHelper,
        IConfiguration configuration,
        UserManager<Usuario> userManager,
        ICustomEmailSender emailSender,
        ILogger<UsuarioService> logger,
        RoleManager<IdentityRole> roleManager,
        IRepository<Suscripcion> repoSubcription,
        IRepository<Reserva> repoReserva,
        IRepository<Pago> repoPagos,
        IRepository<Valoracione> repoValoracion,
        IRepository<Servicio> repoServicio,
        IRepository<ReservasServicio> repoReservaServicio,
        IRepository<Negocio> repoNegocio
    ) : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly ICustomEmailSender _emailSender = emailSender;
        private readonly ILogger<UsuarioService> _logger = logger;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        private readonly IRepository<Suscripcion> _repoSubcription = repoSubcription;
        private readonly IRepository<Reserva> _repoReserva = repoReserva;
        private readonly IRepository<Pago> _repoPagos = repoPagos;
        private readonly IRepository<Valoracione> _repoValoracion = repoValoracion;
        private readonly IRepository<Servicio> _repoServicio = repoServicio;
        private readonly IRepository<ReservasServicio> _repoReservaServicio = repoReservaServicio;
        private readonly IRepository<Negocio> _repoNegocio = repoNegocio; 




        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailAsync(email);
        }


        
        public async Task<LoginResultDTO> Login(string email, string password)
        {
            var errores = new Dictionary<string, string>();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                errores["email"] ="No existe usuario con ese mail";

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                errores["email"] = "Contraseña incorrecta";

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
            // Validar errores personalizados primero
            var errores = await ValidarErroresRegistroAsync(model);
            if (errores.Any())
                throw new ValidationException("Errores de validación en el registro", errores);

            // Verificar si ya existe un usuario con el mismo email
            var existingUser = await _userManager.FindByEmailAsync(model.Email.ToUpperInvariant());
            if (existingUser != null)
            {
                errores["email"] = "El correo electrónico ya está registrado.";
                throw new ValidationException("Errores de validación en el registro", errores);
            }

            var user = new Usuario
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    errores["password"] = errores.ContainsKey("password")
                        ? $"{errores["password"]} {error.Description}"
                        : error.Description;
                }
                throw new ValidationException("Errores de validación al crear el usuario", errores);
            }

            var rol = model.IsNegocio ? ERol.NEGOCIO.ToString() : ERol.CLIENTE.ToString();
            if (string.IsNullOrWhiteSpace(rol))
                throw new InvalidOperationException("El rol no puede ser nulo o vacío.");

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

        public async Task<(bool Success, string Message, List<UsuarioReservaEstadisticaDto> Data)> GetEstadisticasUsuariosPorNegocioAsync(int negocioId)
        {
            try
            {
                var negocio = await _repoNegocio.GetByIdAsync(negocioId);
                if (negocio == null)
                    return (false, $"No existe negocio con Id {negocioId}.", []);

                var usuariosConReservas = from usuario in _usuarioRepository.Query()
                                          join reserva in _repoReserva.Query()
                                              .Where(r => r.NegocioId == negocioId)
                                              on usuario.Id equals reserva.UsuarioId
                                          select new { usuario, reserva };
                var conPagos = from ur in usuariosConReservas
                               join pago in _repoPagos.Query()
                                   on ur.reserva.Id equals pago.ReservaId into pagoGroup
                               from pago in pagoGroup.DefaultIfEmpty()
                               select new { ur.usuario, ur.reserva, pago };
                var conValoraciones = from cp in conPagos
                                      join val in _repoValoracion.Query()
                                          on cp.reserva.Id equals val.ReservaId into valGroup
                                      from val in valGroup.DefaultIfEmpty()
                                      select new { cp.usuario, cp.reserva, cp.pago, val };
                var conServicios = from cv in conValoraciones
                                   join rs in _repoReservaServicio.Query()
                                       on cv.reserva.Id equals rs.ReservaId into rsGroup
                                   from rs in rsGroup.DefaultIfEmpty()
                                   join servicio in _repoServicio.Query()
                                       on rs.ServicioId equals servicio.Id into servicioGroup
                                   from servicio in servicioGroup.DefaultIfEmpty()
                                   select new { cv.usuario, cv.reserva, cv.pago, cv.val, servicio };
                var conSuscripciones = from cs in conServicios
                                       join suscripcion in _repoSubcription.Query()
                                           .Where(s => s.RolNegocio.ToLower() == "cliente")
                                           on cs.usuario.Id equals suscripcion.IdUsuario into subGroup
                                       from suscripcion in subGroup.DefaultIfEmpty()
                                       select new
                                       {
                                           cs.usuario,
                                           cs.reserva,
                                           cs.pago,
                                           cs.val,
                                           cs.servicio,
                                           suscripcion
                                       };
                // Ejecutar la consulta básica y obtener la lista completa en memoria
                var listaCompleta = await conSuscripciones
                    .Where(item => item.reserva != null)
                    .ToListAsync();
                var resultadoFinal = from item in listaCompleta
                                     where item.reserva != null
                                     group item by new
                                     {
                                         item.usuario.Id,
                                         item.usuario.UserName,
                                         item.usuario.Email
                                     } into g
                                     let fechasReservas = g
                                        .Where(x => x.reserva.FechaCreacion != null)
                                        .Select(x => x.reserva.FechaCreacion ?? DateTime.MinValue)
                                        .Distinct()
                                        .OrderBy(f => f)
                                        .ToList()

                                     select new UsuarioReservaEstadisticaDto
                                     {
                                         UsuarioId = g.Key.Id,
                                         Nombre = g.Key.UserName,
                                         Email = g.Key.Email,
                                         TotalReservas = g
                                             .Select(x => x.reserva.Id)
                                             .Distinct()
                                             .Count(),
                                         TotalGastado = g.Sum(x =>
                                             x.pago != null
                                             && x.pago.EstadoPago == "completado"
                                             && x.pago.Reembolsado == false
                                             && x.pago.Monto.HasValue
                                                 ? x.pago.Monto.Value
                                                 : 0m),
                                         FechaPrimeraReserva = fechasReservas.FirstOrDefault(),
                                         FechaUltimaReserva = fechasReservas.LastOrDefault(),
                                         FrecuenciaPromedioDias = fechasReservas.Count >= 2
                                             ? fechasReservas
                                                 .Zip(fechasReservas.Skip(1), (a, b) => (b - a).TotalDays)
                                                 .Average()
                                             : null,
                                         TotalCanceladas = g.Count(x =>
                                             x.reserva.Estado == "Cancelada"),
                                         PuntuacionPromedio = g
                                             .Where(x => x.val != null && x.val.Puntuacion.HasValue)
                                             .Select(x => x.val.Puntuacion.Value)
                                             .DefaultIfEmpty()
                                             .Average(),
                                         ServiciosMasUsados = g
                                             .Where(x => x.servicio != null
                                                         && x.servicio.Nombre != null)
                                             .GroupBy(x => x.servicio.Nombre)
                                             .OrderByDescending(gr => gr.Count())
                                             .Take(3)
                                             .Select(gr => gr.Key)
                                             .ToList(),
                                         EstaSuscrito = g.Any(x => x.suscripcion != null)
                                     };
                var result = resultadoFinal.ToList();
                return (true, "OK", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetEstadisticasUsuariosPorNegocioAsync");
                _logger.LogError(ex, ex.Message);
                _logger.LogError(ex, ex.InnerException?.Message);
                return (false, "Error interno", new List<UsuarioReservaEstadisticaDto>());
            }
        }

        public async Task<Dictionary<string, string>> ValidarErroresRegistroAsync(RegisterDTO model)
        {
            var errores = new Dictionary<string, string>();

            // Validar nombre de usuario
            if (string.IsNullOrWhiteSpace(model.UserName))
                errores["username"] = "El nombre de usuario no puede estar vacío.";
            var userExist = await _usuarioRepository.Exist(item =>
                item.NormalizedEmail.Equals(model.Email.ToUpperInvariant()));
            if (userExist)
                errores["email"] = "El email ya existe";
            
            // Validar email
            if (string.IsNullOrWhiteSpace(model.Email))
                errores["email"] = "El correo electrónico no puede estar vacío.";
            else if (!model.Email.Contains("@"))
                errores["email"] = "El correo electrónico no es válido.";

            // Validar número de teléfono
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                errores["phoneNumber"] = "El número de teléfono no puede estar vacío.";

            // Validar contraseña usando UserManager y sus validadores registrados
            var fakeUser = new Usuario { UserName = model.UserName, Email = model.Email };
            var passwordValidation = await _userManager.PasswordValidators[0].ValidateAsync(_userManager, fakeUser, model.Password);

            if (!passwordValidation.Succeeded)
            {
                var passwordErrors = passwordValidation.Errors.Select(e => e.Description).ToList();
                errores["password"] = string.Join(" ", passwordErrors);
            }
            return errores;
        }
    }
}
