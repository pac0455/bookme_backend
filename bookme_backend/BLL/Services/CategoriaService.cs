using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;

namespace bookme_backend.BLL.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly IRepository<Categoria> _repository;
        public CategoriaService(IRepository<Categoria> repository)
        {
            _repository = repository;
        }
        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            var categorias = await _repository.GetAllAsync();
            return categorias;
        }
    }
}
