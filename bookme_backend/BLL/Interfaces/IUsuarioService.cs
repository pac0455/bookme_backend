using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Interfaces
{
    public interface IUsuarioService 
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> ObtenerPorFirebaseUidAsync(string uid);
        Task<Usuario> CrearUsuarioAsync(Usuario usuario);
        Task Update(Usuario usuario);
        Task<Usuario> DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<Usuario?> GetByEmailAsync(string email);
    }
}
