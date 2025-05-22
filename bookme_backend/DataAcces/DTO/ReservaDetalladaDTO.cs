namespace bookme_backend.DataAcces.DTO
{
    public class ReservaDetalladaDto
    {
        public int ReservaId { get; set; }
        public DateOnly? Fecha { get; set; }
        public string? Estado { get; set; }
        public string? ComentarioCliente { get; set; }
        public List<ServicioConPagoDto> Servicios { get; set; } = new();
        public double TotalReserva => Servicios.Sum(s => s.Precio ?? 0.0);
        public string EstadoPagoGeneral { get; set; } = "sin pago";
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
        public string Estado { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
    }
}
