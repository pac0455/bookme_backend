using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> GetCategoriasAsync();
    }
}
