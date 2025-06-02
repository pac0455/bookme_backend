namespace bookme_backend.DataAcces.DTO.Valoraciones
{
    public class ValoracionResponseDTO
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public string UsuarioId { get; set; } = null!;
        public double Puntuacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaValoracion { get; set; }

        public UsuarioDTO Usuario { get; set; } = null!;
    }

    public class UsuarioDTO
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

}
