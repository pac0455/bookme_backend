using System.ComponentModel.DataAnnotations;

namespace bookme_backend.DataAcces.DTO.Valoraciones
{
    public class ValoracionCreateDTO
    {
        [Required]
        public int NegocioId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        [Range(0, 5)]
        public double Puntuacion { get; set; }

        public string? Comentario { get; set; }
    }
}
