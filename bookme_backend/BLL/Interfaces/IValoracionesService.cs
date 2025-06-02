using bookme_backend.DataAcces.DTO.Valoraciones;

namespace bookme_backend.BLL.Interfaces
{
    public interface IValoracionesService
    {
        Task<(bool Succes, string Message, List<ValoracionResponseDTO> valoraciones)> GetValoracionesByNegocioId(int negocioID);
        Task<(bool Succes, string Message,ValoracionResponseDTO valoracionCreada)> Create(ValoracionCreateDTO valoracion);
    }
}
