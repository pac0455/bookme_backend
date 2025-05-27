using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("valoraciones")]
public partial class Valoracion
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("reserva_id")]
    public int ReservaId { get; set; }

    [Column("usuario_id")]
    public string UsuarioId { get; set; } = null!;

    [Column("puntuacion")]
    public int? Puntuacion { get; set; }

    [Column("comentario")]
    public string? Comentario { get; set; }

    [Column("fecha_valoracion", TypeName = "datetime")]
    public DateTime? FechaValoracion { get; set; }

    [ForeignKey("ReservaId")]
    [InverseProperty("Valoraciones")]
    public virtual Reserva Reserva { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Valoraciones")]
    public virtual Usuario Usuario { get; set; } = null!;
}
