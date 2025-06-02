using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bookme_backend.DataAcces.Models;

[Table("Valoraciones")]
public class Valoracion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("negocio_id")]
    public int NegocioId { get; set; }

    [Column("usuario_id")]
    public string UsuarioId { get; set; } = null!;

    [Column("puntuacion")]
    public double Puntuacion { get; set; }

    [Column("comentario")]
    public string? Comentario { get; set; }

    [Column("fecha_valoracion")]
    public DateTime FechaValoracion { get; set; }

    [ForeignKey(nameof(NegocioId))]
    public virtual Negocio Negocio { get; set; } = null!;

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;
}