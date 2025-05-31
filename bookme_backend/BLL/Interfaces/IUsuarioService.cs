using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.Models;


namespace bookme_backend.BLL.Interfaces
{
    public interface IUsuarioService 
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid);
        Task Update(Usuario usuario);
        Task<(bool Success, string Message)> DeleteAsync(string email);
        Task SaveChangesAsync();
        Task<Usuario?> GetByEmailAsync(string email);
        Task<LoginResultDTO> Login(string email, string password);
        Task<LoginResultDTO> RegisterAsync(RegisterDTO model);
        Task<(bool Success, string Message)> ResendConfirmationEmailAsync(string email);
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string resetPasswordBaseUrl);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<(bool Success, string Message, List<UsuarioReservaEstadisticaDto> Data)> GetEstadisticasUsuariosPorNegocioAsync(int negocioId);
        Task<Dictionary<string, string>> ValidarErroresRegistroAsync(RegisterDTO model);
        Task<Dictionary<string, string>> ValidarErroresLoginAsync(string email, string password);

        Task<bool> UsuarioTieneServiciosAsync(int usuarioId);
    }
}
