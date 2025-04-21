using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.Repositories.Interfaces
{
    public interface IUsuarioRepository: IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario> GetByFirebaseUidAsync(string uid);
        Task SaveChangesAsync();
    }

}
