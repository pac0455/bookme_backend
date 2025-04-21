using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.BLL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository; // Usamos IRepository<Usuario> ahora

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Task<List<Usuario>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario> RegistrarConGoogleAsync(string firebaseIdToken)
        {
            try
            {
                // 1. Verificar el token
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(firebaseIdToken);
                string uid = decodedToken.Uid;

                // 2. Obtener más datos del usuario desde Firebase
                UserRecord firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

                // 3. Buscar en la base de datos si ya existe
                var usuarioExistente = await _usuarioRepository.GetByFirebaseUidAsync(uid);
                if (usuarioExistente != null)
                {
                    return usuarioExistente;
                }

                // 4. Si no existe, crearlo localmente
                var nuevoUsuario = new Usuario
                {
                    Nombre = firebaseUser.DisplayName,
                    Email = firebaseUser.Email,
                    Telefono = firebaseUser.PhoneNumber,
                    FirebaseUid = uid,
                    FechaRegistro = DateTime.UtcNow,
                    Rol = "usuario"
                };

                await _usuarioRepository.AddAsync(nuevoUsuario);
                await _usuarioRepository.SaveChangesAsync();

                return nuevoUsuario;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar usuario con Google", ex);
            }
        }

        public async Task<Usuario> RegistrarUsuarioAsync(string nombre, string email, string telefono, string contrasena)
        {
            try
            {
                // 1. Crear usuario en Firebase
                var args = new UserRecordArgs()
                {
                    Email = email,
                    Password = contrasena,
                    DisplayName = nombre,
                    PhoneNumber = telefono
                };

                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);

                // 2. Crear el objeto Usuario para la base de datos local
                var usuario = new Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Telefono = telefono,
                    FirebaseUid = userRecord.Uid,
                    FechaRegistro = DateTime.UtcNow,
                    Rol = "usuario", // o el rol que definas
                    ContrasenaHash = null // Si usas Firebase, no necesitas guardar la contraseña
                };

                // 3. Guardar el usuario en la base de datos utilizando el repositorio genérico
                await _usuarioRepository.AddAsync(usuario);

                return usuario;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (puedes personalizar el mensaje o registrar el error)
                throw new Exception("Error al registrar el usuario.", ex);
            }
        }
    }


}
