using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Interfaces
{
    public interface IHorarioService
    {
        Task<(bool Success, string Message)> AddRangeAsync(List<Horario> horarios);
        Task<(bool Success, string Message, List<Horario> horarios)> GetHorariosByNegocioId(int negocioId);

        Task<(bool Succes, string Message, List<Horario> horario)> GetHorarioServicioSinReservaByNegocioID(int negocioId, int servicoId, DateOnly date);
    }
}
