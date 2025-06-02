namespace bookme_backend.DataAcces.DTO.Reserva
{
    public class ReservaResponseNegocio
    {
        string Username { get; set; }
        string NReservasUsuario { get; set; }
        string ServicioNombre { get; set; }
        string ServicioDescrpcion { get; set; }
        string ServicioEstado { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }


    }
}
