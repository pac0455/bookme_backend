using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("usuarios")]
[Index("Email", Name = "UQ__usuarios__AB6E6164E28DE90C", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string? Nombre { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("telefono")]
    [StringLength(255)]
    public string? Telefono { get; set; }

    [Column("contrasena_hash")]
    [StringLength(255)]
    public string? ContrasenaHash { get; set; }

    [Column("firebase_uid")]
    [StringLength(255)]
    public string? FirebaseUid { get; set; }

    [Column("rol")]
    [StringLength(255)]
    public string? Rol { get; set; }

    [Column("fecha_registro", TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Valoracione> Valoraciones { get; set; } = new List<Valoracione>();
}
