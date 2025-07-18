﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("negocios")]
public partial class Negocio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(255), Required]
    public string Nombre { get; set; }

    [Column("descripcion"), Required]
    [StringLength(255)]
    public string Descripcion { get; set; }

    [Column("direccion")]
    [StringLength(255), Required]
    public string Direccion { get; set; }

    [Column("logo")]
    [StringLength(255)]
    public string? LogoUrl { get; set; }

    [Column("latitud"), Required]
    public double? Latitud { get; set; }
    [Column("longitud"), Required]
    public double? Longitud { get; set; }
    [Column("categoria"), Required]
    public int CategoriaId { get; set; }

    [ForeignKey("CategoriaId")] 
    public virtual Categoria? Categoria { get; set; }

    [Column("logo_updated_at")] 
    public long? LogoUpdatedAt { get; set; } 

    [Column("activo")]
    public bool Activo { get; set; }

    [Column("eliminado")]
    public bool Eliminado { get; set; } = false;
    public bool Bloqueado { get; set; } = false;

    [InverseProperty("Negocio")]
    [JsonPropertyName("horarioAtencion")]
    public virtual ICollection<Horario> HorariosAtencion { get; set; } = new List<Horario>();
    [JsonIgnore]
    [InverseProperty("Negocio")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    [JsonIgnore]
    [InverseProperty("Negocio")]
    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    [JsonIgnore]
    [InverseProperty("IdNegocioNavigation")]
    public virtual ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
    [JsonIgnore]
    [InverseProperty("Negocio")]
    public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
    public override string ToString()
    {
        return $"Negocio {{ " +
               $"Id = {Id}, " +
               $"Nombre = {Nombre}, " +
               $"Descripcion = {Descripcion}, " +
               $"Direccion = {Direccion}, " +
               $"LogoUrl = {LogoUrl}, " +
               $"Latitud = {Latitud}, " +
               $"Longitud = {Longitud}, " +
               $"CategoriaId = {CategoriaId}, " +
               $"Categoria = {(Categoria != null ? Categoria.Nombre : "null")}, " +
               $"Activo = {Activo} " +
               $"}}";
    }

}
