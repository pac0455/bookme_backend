using Azure.Core;
using bookme_backend.BLL.Exceptions;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Security.Policy;

namespace bookme_backend.BLL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHelper _passwordHelper;

        public UsuarioService(IUsuarioRepository usuarioRepository, IPasswordHelper passwordHelper)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHelper = passwordHelper;
        }
        public async Task<Usuario?> GetByEmailAsync(string email){
            return await _usuarioRepository.GetByEmailAsync(email);
        }
        public async Task<Usuario?> Login(string email, string password)
        {
            // Buscar el usuario por email
            var user = await _usuarioRepository.GetByEmailAsync(email);

            if (user == null)
                throw new KeyNotFoundException("Usuario no encontrado.");

            // Verificar contraseña con PasswordHelper
            var isPasswordValid = _passwordHelper.VerifyPassword(user, user.PasswordHash, password);

            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Contraseña incorrecta.");

            return user;
        }



        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _usuarioRepository.GetAllAsync();
        }
        

        public async Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid)
        {
            return await _usuarioRepository.GetByFirebaseUidAsync(uid);
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario user)
        {
            var existe = await _usuarioRepository.Exist(u => u.Email == user.Email || u.FirebaseUid == user.FirebaseUid);

            if (existe) {
                throw new EntityDuplicatedException("La entidad ya existe");
            }
            user.PasswordHash = _passwordHelper.HashPassword(user, user.PasswordHash);
            await _usuarioRepository.AddAsync(user);

            await _usuarioRepository.SaveChangesAsync();
            return user;
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

        public Task<Usuario> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}
