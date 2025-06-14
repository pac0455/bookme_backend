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
    public static class ReservaNegocioMapper
    {
        public static ReservaResponseNegocioDTO FromEntity(Models.Reserva reserva, Models.Usuario usuario)
        {
            var fechaFinReserva = reserva.Fecha.ToDateTime(reserva.HoraFin);
            bool esFinalizada = DateTime.Now > fechaFinReserva;

            return new ReservaResponseNegocioDTO
            {
                Id = reserva.Id,
                Username = usuario?.UserName ?? "Usuario sin nombre",
                NReservasUsuario = 0, // se llenará después,
                ServicioNombre = reserva.Servicio?.Nombre ?? "Servicio sin nombre",
                ServicioDescripcion = reserva.Servicio?.Descripcion ?? "Sin descripcion",
                Fecha = reserva.Fecha,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Precio = (double)(reserva.Servicio?.Precio ?? 0),
                Moneda = reserva.Pago?.Moneda ?? "EUR",
                EstadoReserva = reserva.Estado == EstadoReserva.Cancelada
                    ? EstadoReserva.Cancelada
                    : (esFinalizada ? EstadoReserva.Finalizada : reserva.Estado),
                EstadoPago = reserva.Pago?.EstadoPago ?? EstadoPago.Pendiente
            };
        }
    }
}
