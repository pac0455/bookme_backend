    namespace bookme_backend.DataAcces.DTO.Pago
    {
        public class PagoCreateDTO
        {
            public decimal Monto { get; set; }
            public MetodoPago MetodoPago { get; set; }
            public string? Moneda { get; set; } = "EUR"; // Valor por defecto

            // Campos opcionales que el backend puede asignar
            public EstadoPago? EstadoPago { get; set; } // El backend lo asignará según resultado
            public string? RespuestaPasarela { get; set; } // Lo llenará la pasarela simulada
        }
    }