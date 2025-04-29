using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using Humanizer;
using Microsoft.AspNetCore.Identity;



namespace bookme_backend.BLL.Services
{
    public class PasswordHelper : IPasswordHelper
    {
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public PasswordHelper(IPasswordHasher<Usuario> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(Usuario user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(Usuario user, string hashedPassword, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
