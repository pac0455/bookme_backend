using System.ComponentModel.DataAnnotations;
using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaCreateDTO
    {
        public int NegocioId { get; set; }
        public string UsuarioId { get; set; } = null!;   // Non-nullable string
        public DateTime Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public EstadoReserva? Estado { get; set; }       // Nullable enum para opcionalidad

        public int ServicioId { get; set; }              // Cambiado a un solo servicioId
        public required PagoCreateDTO Pago { get; set; }
    }
}
