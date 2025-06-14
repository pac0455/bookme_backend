using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Usuario;
using bookme_backend.DataAcces.Models;
using Microsoft.AspNetCore.Identity;


namespace bookme_backend.BLL.Interfaces
{
    public interface IUsuarioService
    {
        // GET
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid);
        Task<(bool Success, string Message, List<ClienteResumenDTO> Data)> GetEstadisticasUsuariosPorNegocioAsync(int negocioId);
        Task<bool> UsuarioTieneServiciosAsync(int usuarioId);
        Task<List<UsuarioAdminDTO>> GetUsuariosNOAdminDTOAsync();

        // POST
        Task<LoginResultDTO> Login(string email, string password);
        Task<LoginResultDTO> RegisterAsync(RegisterDTO model);
        Task CreateAdminUserAsync(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager);
        Task<(bool Success, string Message)> ResendConfirmationEmailAsync(string email);
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string resetPasswordBaseUrl);
        Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword);
        Task<Dictionary<string, string>> ValidarErroresRegistroAsync(RegisterDTO model);
        Task<Dictionary<string, string>> ValidarErroresLoginAsync(string email, string password);
        Task<(bool Success, string Message)> SendAuthenticationCodeAsync(string id);
        Task<(bool Success, string Message)> VerifyCodeAsync(string userId, string code);


        // PUT / UPDATE

        Task<(bool Success, string Message)> BloquearUsuarioAsync(string id);
        Task<(bool Success, string Message)> DesbloquearUsuarioAsync(string id);
        Task Update(Usuario usuario);
        Task<(bool Success, UpdateNombreDTO usuarioActualizado, string Message)> UpdateUsuarioNombreAsync(UpdateNombreDTO newUser);
        Task<(bool Success, string Message)> UpdatePasswordAsync(UpdatePasswordDTO model);
        Task SaveChangesAsync();

        // DELETE
        Task<(bool Success, string Message)> DeleteAsync(string email);



    }
}

