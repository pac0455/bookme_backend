using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("negocios")]
public partial class Negocio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string? Nombre { get; set; }

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("direccion")]
    [StringLength(255)]
    public string? Direccion { get; set; }

    [Column("latitud")]
    public double? Latitud { get; set; }

    [Column("longitud")]
    public double? Longitud { get; set; }

    [Column("categoria")]
    [StringLength(255)]
    public string? Categoria { get; set; }
    [Column("activo")]
    public bool? Activo { get; set; }

    [InverseProperty("Negocio")]
    public virtual ICollection<Horarios> HorariosAtencion { get; set; } = new List<Horarios>();

    [InverseProperty("Negocio")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    [InverseProperty("Negocio")]
    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();

    [InverseProperty("IdNegocioNavigation")]
    public virtual ICollection<Suscripcione> Suscripciones { get; set; } = new List<Suscripcione>();
}
