using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaResponseNegocioDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int NReservasUsuario { get; set; }
        public string ServicioNombre { get; set; }
        public string ServicioDescripcion { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public double Precio { get; set; }
        public string Moneda { get; set; }
        public EstadoReserva EstadoReserva { get; set; }
        public EstadoPago EstadoPago { get; set; }
    }
}
