using Azure.Core;
using bookme_backend.BLL.Exceptions;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Reserva;
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
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRepository<Negocio> _negocioRepository;
        private readonly IRepository<Reserva> _reservaRepository;
        private readonly IRepository<Valoracion> _valoracionRepository;
        private readonly IRepository<RolGlobal> _rolGlobalRepository;
        private readonly IRepository<Suscripcion> _suscripcionRepository;
        private readonly IRepository<Pago> _pagoRepo;
        private readonly AuthCodeStore _authCodeStore;


        private readonly IConfiguration _configuration;
        private readonly UserManager<Usuario> _userManager;
        private readonly ICustomEmailSender _emailSender;
        private readonly ILogger<UsuarioService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IRepository<Negocio> negocioRepository,
            IRepository<Reserva> reservaRepository,
            IRepository<Valoracion> valoracionRepository,
            IRepository<RolGlobal> rolGlobalRepository,
            IRepository<Suscripcion> suscripcionRepository,
            IConfiguration configuration,
            UserManager<Usuario> userManager,
            ICustomEmailSender emailSender,
            ILogger<UsuarioService> logger,
            AuthCodeStore authCodeStore,
            IRepository<Pago> pagoRepo,
            RoleManager<IdentityRole> roleManager)
        {
            _usuarioRepository = usuarioRepository;
            _negocioRepository = negocioRepository;
            _reservaRepository = reservaRepository;
            _valoracionRepository = valoracionRepository;
            _rolGlobalRepository = rolGlobalRepository;
            _suscripcionRepository = suscripcionRepository;
            _configuration = configuration;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _roleManager = roleManager;
            _pagoRepo = pagoRepo;
            _authCodeStore = authCodeStore;
        }





        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _usuarioRepository.GetByEmailAsync(email);
        }


        //Solo se podrá logear como admin en login ya que no tiene sentido que se registre como admin
        // en la misma aplicación, ya que el admin se crea desde la base de datos.
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
                Roles = roles,
            };

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


            result = await _userManager.AddToRoleAsync(user, model.IsNegocio ? ERol.NEGOCIO.ToString() :ERol.CLIENTE.ToString());

            if (!result.Succeeded)
            {
                // Esto ya no debería ocurrir si ValidarErroresRegistroAsync valida correctamente.
                foreach (var error in result.Errors)
                {
                    errores["password"] = errores.ContainsKey("password")
                        ? $"{errores["password"]} {error.Description}"
                        : error.Description;
                }
                throw new ValidationException("Errores de validación al crear el usuario", errores);
            }
            //Sustituir para añadir los roles por una seed
            //if (!await _roleManager.RoleExistsAsync(rol))
            //    await _roleManager.CreateAsync(new IdentityRole(rol));

            //await _userManager.AddToRoleAsync(user, rol);

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
            // Obtener todos los usuarios
            var todosLosUsuarios = await _userManager.Users.ToListAsync();

            // Obtener los usuarios con el rol ADMIN
            var usuariosAdmin = await _userManager.GetUsersInRoleAsync(ERol.ADMIN.ToString());

            // Excluir los que están en la lista de admins
            var usuariosNoAdmin = todosLosUsuarios
                .Where(u => !usuariosAdmin.Any(admin => admin.Id == u.Id))
                .ToList();

            return usuariosNoAdmin;
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
            var usuario = await _userManager.FindByEmailAsync(email);


            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var resultado = await _userManager.DeleteAsync(usuario);
            // Obtener suscripciones del usuario
            var suscripciones = await _suscripcionRepository.GetWhereAsync(s => s.IdUsuario == usuario.Id);
            var negocioIds = suscripciones.Select(s => s.IdNegocio).Distinct().ToList();

            // Obtener los negocios asociados
            var negocios = await _negocioRepository.GetWhereAsync(n => negocioIds.Contains(n.Id));

            // Iterar sobre cada negocio y cancelar reservas y pagos asociados
            foreach (var negocio in negocios)
            {
                negocio.Activo = false;
                await CancelarReservasDeNegocio(negocio.Id, "Usuario bloqueado");
                await CancelarPagosDeNegocio(negocio.Id, "Usuario bloqueado");
            }
            await _negocioRepository.SaveChangesAsync();

            if (resultado.Succeeded)
            {
                return (true, "Usuario eliminado correctamente.");
            }
            

            var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
            return (false, $"Error al eliminar el usuario: {errores}");
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
            usuario.PhoneNumber = newUser.Telefono;
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

        public async Task CreateAdminUserAsync(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminRoleName = ERol.ADMIN.ToString();
            string adminEmail = "admin@example.com";
            string adminPassword = "AdminPass123!";

            //  Crear rol admin si no existe
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            // Verificar si el usuario ya existe
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                //Crear usuario admin
                adminUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createUserResult.Succeeded)
                {
                    throw new Exception("No se pudo crear el usuario admin: " + string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
            }

            // Asignar rol admin al usuario (si no lo tiene ya)
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);

                if (!addRoleResult.Succeeded)
                {
                    throw new Exception("No se pudo asignar el rol admin al usuario: " + string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                }
            }
        }
        public async Task<List<UsuarioAdminDTO>> GetUsuariosNOAdminDTOAsync()
        {
            // Para obtener IQueryable desde los repos
            var usuariosQuery = _usuarioRepository.Query();
            var rolesGlobalesQuery = _rolGlobalRepository.Query();
            var negociosQuery = _negocioRepository.Query();
            var reservasQuery = _reservaRepository.Query();
            var valoracionesQuery = _valoracionRepository.Query();
            var suscripcionesQuery = _suscripcionRepository.Query();
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly date30DaysAgo = today.AddDays(-30);
            var query = from u in usuariosQuery
                        join rg in rolesGlobalesQuery on u.Id equals rg.UsuarioId
                        where !rolesGlobalesQuery.Any(rg => rg.UsuarioId == u.Id && rg.Rol == ERol.ADMIN)
                        select new UsuarioAdminDTO
                        {
                            Id = u.Id,
                            Email = u.Email,
                            NegociosActivos = negociosQuery.Count(n => n.Activo),
                            NegociosBloqueados = negociosQuery.Count(n => n.Bloqueado),
                            TotalUsuarios = usuariosQuery.Count(),
                            ReservasUltimos30Dias = reservasQuery.Count(r => r.UsuarioId == u.Id && r.Fecha >= date30DaysAgo),
                            TotalValoraciones = valoracionesQuery.Count(v => v.UsuarioId == u.Id),
                            PromedioPuntuacion = valoracionesQuery.Where(v => v.UsuarioId == u.Id).Average(v => (double?)v.Puntuacion) ?? 0,
                            NumeroNegocios = suscripcionesQuery.Count(s => s.IdUsuario == u.Id && s.RolNegocio == ERol.NEGOCIO.ToString())
                        };
            return await query.ToListAsync();
        }

       

        public async Task<(bool Success, string Message)> BloquearUsuarioAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            // Bloquear el usuario
            usuario.Bloqueado = true;
            var result = await _userManager.UpdateAsync(usuario);

            if (!result.Succeeded)
            {
                var errores = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, $"Error al bloquear el usuario: {errores}");
            }


            // Obtener suscripciones del usuario
            var suscripciones = await _suscripcionRepository.GetWhereAsync(s => s.IdUsuario == usuario.Id);
            var negocioIds = suscripciones.Select(s => s.IdNegocio).Distinct().ToList();

            // Obtener los negocios asociados
            var negocios = await _negocioRepository.GetWhereAsync(n => negocioIds.Contains(n.Id));

            // Iterar sobre cada negocio y cancelar reservas y pagos asociados
            foreach (var negocio in negocios)
            {
                negocio.Activo = false;
                await CancelarReservasDeNegocio(negocio.Id, "Usuario bloqueado");
                await CancelarPagosDeNegocio(negocio.Id, "Usuario bloqueado");
            }
            await _negocioRepository.SaveChangesAsync();

            return (true, "Usuario bloqueado y reservas/pagos asociados cancelados correctamente.");
        }


        public async Task<(bool Success, string Message)> DesbloquearUsuarioAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            // Bloquear el usuario
            usuario.Bloqueado = false;
            var result = await _userManager.UpdateAsync(usuario);

            // Obtener suscripciones del usuario
            var suscripciones = await _suscripcionRepository.GetWhereAsync(s => s.IdUsuario == usuario.Id);
            var negocioIds = suscripciones.Select(s => s.IdNegocio).Distinct().ToList();
     


            if (!result.Succeeded)
            {
                var errores = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, $"Error al desbloquear el usuario: {errores}");
            }
            return (true, "Usuario desbloqueado.");
        }
        // Método para cancelar reservas de un negocio (sin llamada a cancelar pagos aquí)
        private async Task<List<Reserva>> CancelarReservasDeNegocio(int negocioId, string motivo)
        {
            var reservas = (await _reservaRepository.GetWhereAsync(r => r.NegocioId == negocioId && r.Estado != EstadoReserva.Cancelada)).ToList();

            foreach (var reserva in reservas)
            {
                reserva.Estado = EstadoReserva.Cancelada;
                reserva.CancelacionMotivo = motivo;
                _reservaRepository.Update(reserva);
            }

            await _reservaRepository.SaveChangesAsync();

            return reservas;
        }

        // Método independiente que cancela/reembolsa pagos pendientes de un negocio, sin requerir reservas externas
        private async Task CancelarPagosDeNegocio(int negocioId, string motivo)
        {
            // Obtiene las reservas activas o canceladas que tengan pagos no reembolsados
            var reservasConPagos = (await _reservaRepository.GetWhereWithIncludesAsync(r =>
                r.NegocioId == negocioId && r.Pago != null && r.Pago.EstadoPago != EstadoPago.Reembolsado,
                r => r.Pago
            )).ToList();

            foreach (var reserva in reservasConPagos)
            {
                reserva.Pago.EstadoPago = EstadoPago.Reembolsado;
                // Opcional: podrías agregar motivo de reembolso en pago, si tienes campo para eso
                _pagoRepo.Update(reserva.Pago);
            }

            await _pagoRepo.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> SendAuthenticationCodeAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Guardamos el código con TTL de 10 minutos
            _authCodeStore.SaveCode(user.Id, code, TimeSpan.FromMinutes(10));

            var htmlContent = $@"
            <html>
              <body>
                <p>Tu código de verificación es: <strong>{code}</strong></p>
                <p>Introduce este código en la app para continuar.</p>
              </body>
            </html>";

            await _emailSender.SendEmailAsync(user.Email, "Código de verificación", htmlContent);

            return (true, "Código enviado correctamente.");
        }

        public bool VerifyAuthenticationCode(string userId, string inputCode)
        {
            return _authCodeStore.VerifyCode(userId, inputCode);
        }

        public async Task<(bool Success, string Message)> VerifyCodeAsync(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                return (false, "UserId y código son requeridos.");
            }

            bool isValid = _authCodeStore.VerifyCode(userId, code);

            if (isValid)
            {
                var user = await _userManager.FindByIdAsync(userId);

                user.EmailConfirmed = true; // Confirmamos el email del usuario

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errores = string.Join("; ", result.Errors.Select(e => e.Description));
                    return (false, $"Error al verificar el código: {errores}");
                }


                // Si el código es válido, lo eliminamos del almacenamiento
                return (true, "Código verificado correctamente.");
            }
            else
            {
                // Si el código es válido, lo eliminamos del almacenamiento
                return (false, "Código inválido o expirado.");
            }
        }

    }
}
