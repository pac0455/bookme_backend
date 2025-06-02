using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaDetalladaDto
    {
        public int ReservaId { get; set; }
        public DateOnly Fecha { get; set; }
        public EstadoReserva Estado { get; set; }
        public string? ComentarioCliente { get; set; }
        public List<ServicioConPagoDto> Servicios { get; set; } = new();
        public double TotalReserva => Servicios.Sum(s => s.Precio ?? 0.0);
        public EstadoPago EstadoPagoGeneral { get; set; }
    }

    public class ServicioConPagoDto
    {
        public string? Nombre { get; set; }
        public double? Precio { get; set; }
        public PagoDto? Pago { get; set; }
    }

    public class PagoDto
    {
        public double Monto { get; set; }
        public EstadoPago Estado { get; set; } 
        public MetodoPago Metodo { get; set; }
        public DateTime FechaPago { get; set; }
    }
}
