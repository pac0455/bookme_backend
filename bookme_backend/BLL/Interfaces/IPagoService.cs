namespace bookme_backend.BLL.Interfaces
{
    public interface IPagoService
    {
        Task<(bool Success, string Message)> CambiarEstadoPagoAsync(int reservaId, int nuevoEstado);

    }
}
