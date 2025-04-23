using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("suscripciones")]
public partial class Suscripcione
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_negocio")]
    public int IdNegocio { get; set; }

    [Column("id_usuario")]
    public string IdUsuario { get; set; } = null!;

    [Column("fecha_suscripcion", TypeName = "datetime")]
    public DateTime FechaSuscripcion { get; set; }

    [Column("rol_negocio")]
    [StringLength(255)]
    public string? RolNegocio { get; set; }

    [ForeignKey("IdNegocio")]
    [InverseProperty("Suscripciones")]
    public virtual Negocio IdNegocioNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Suscripciones")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
