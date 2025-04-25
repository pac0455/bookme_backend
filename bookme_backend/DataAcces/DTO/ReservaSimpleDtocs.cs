namespace bookme_backend.DataAcces.DTO
{
    public class ReservaSimpleDto
    {
        public int NegocioId { get; set; }
        public string UsuarioId { get; set; } = null!;
        public DateOnly? Fecha { get; set; }
        public TimeOnly? HoraInicio { get; set; }
        public TimeOnly? HoraFin { get; set; }
        public string? Estado { get; set; }
        public string? ComentarioCliente { get; set; }
        public DateTime? FechaCreacion { get; set; }
    }

}
