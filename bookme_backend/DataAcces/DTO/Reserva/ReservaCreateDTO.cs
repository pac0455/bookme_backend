using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaCreateDTO
    {
        public int NegocioId { get; set; }
        public string UsuarioId { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public EstadoReserva Estado { get; set; }
        public string? ComentarioCliente { get; set; }
        public List<int> ServicioIds { get; set; } = [];
        public PagoCreateDTO? Pago { get; set; }
    }

}
