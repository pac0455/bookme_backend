using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaResponseDTO
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public string UsuarioId { get; set; } = null!;
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public EstadoReserva Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int ServicioId { get; set; }

        public ServicioDTO Servicio { get; set; } = null!;
        public PagoDTO? Pago { get; set; }
    }

    public class ServicioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
    }

    public class PagoDTO
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public EstadoPago EstadoPago { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public DateTime Creado { get; set; }
    }

    public static class ReservaMapper
    {
        public static ReservaResponseDTO FromEntity(Models.Reserva reserva, Models.Servicio servicio, Models.Pago? pago)
        {
            return new ReservaResponseDTO
            {
                Id = reserva.Id,
                NegocioId = reserva.NegocioId,
                UsuarioId = reserva.UsuarioId,
                Fecha = reserva.Fecha,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Estado = reserva.Estado,
                FechaCreacion = reserva.FechaCreacion,
                ServicioId = reserva.ServicioId,
                Servicio = new ServicioDTO
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre
                },
                Pago = pago == null ? null : new PagoDTO
                {
                    Id = pago.Id,
                    Monto = pago.Monto,
                    EstadoPago = pago.EstadoPago,
                    MetodoPago = pago.MetodoPago,
                    Creado = pago.FechaPago ?? DateTime.MinValue
                }
            };
        }
    }
}
