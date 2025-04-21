using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario> RegistrarUsuarioAsync(string nombre, string email, string telefono, string contrasena);
        Task<Usuario> RegistrarConGoogleAsync(string firebaseIdToken);
        Task<List<Usuario>> GetAllAsync();

    }
}
