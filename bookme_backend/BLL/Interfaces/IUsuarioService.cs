using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Interfaces
{
    public interface IUsuarioService 
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid);
        Task<Usuario> CrearUsuarioAsync(Usuario usuario, string passwrod);
        Task Update(Usuario usuario);
        Task<(bool Success, string Message)> DeleteAsync(string id);
        Task SaveChangesAsync();
        Task<Usuario?> GetByEmailAsync(string email);
        Task<LoginResultDTO> Login(string email, string password);
        Task<LoginResultDTO> RegisterAsync(RegisterDTO model);



        Task<(bool Success, string Message)> ResendConfirmationEmailAsync(string email);

        Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string resetPasswordBaseUrl);

        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
