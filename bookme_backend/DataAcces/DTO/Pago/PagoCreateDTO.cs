namespace bookme_backend.DataAcces.DTO.Pago
{
    public class PagoCreateDTO
    {
        public decimal Monto { get; set; }
        public string EstadoPago { get; set; } = null!;
        public string MetodoPago { get; set; } = null!;
        public string? IdTransaccionExterna { get; set; }
        public string? RespuestaPasarela { get; set; }
        public string? Moneda { get; set; }
    }

}
