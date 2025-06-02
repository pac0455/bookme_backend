namespace bookme_backend.DataAcces.DTO.Reserva
{
    public enum EstadoReserva
    {
        Pendiente,   // La reserva fue creada y está a la espera de que llegue la fecha del servicio
        Finalizada,  // El servicio ya fue prestado
        Cancelada    // La reserva fue cancelada por el cliente o el negocio
    }
}
