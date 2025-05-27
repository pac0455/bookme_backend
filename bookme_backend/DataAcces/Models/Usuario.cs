using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("usuarios")]
[Index("Email", Name = "UQ__usuarios__AB6E6164E28DE90C", IsUnique = true)]
[Index(nameof(FirebaseUid), Name = "UQ__usuarios__AB6E6164E28DE90S", IsUnique = true)]
public partial class Usuario:  IdentityUser
{
    [Column("firebase_uid")]
    [StringLength(255)]
    public string? FirebaseUid { get; set; } = null;
    //Relaciones
    [InverseProperty("Usuario")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
}
