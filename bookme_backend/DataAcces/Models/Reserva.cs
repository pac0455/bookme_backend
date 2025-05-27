using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("reservas")]
public partial class Reserva
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("negocio_id")]
    public int NegocioId { get; set; }

    [Column("usuario_id")]
    public string UsuarioId { get; set; } = null!;

    [Column("fecha")]
    public DateOnly? Fecha { get; set; }

    [Column("hora_inicio")]
    public TimeOnly? HoraInicio { get; set; }

    [Column("hora_fin")]
    public TimeOnly? HoraFin { get; set; }

    [Column("estado")]
    [StringLength(255)]
    public string? Estado { get; set; }

    [Column("comentario_cliente")]
    public string? ComentarioCliente { get; set; }

    [Column("fecha_creacion", TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("NegocioId")]
    [InverseProperty("Reservas")]
    [JsonIgnore]
    public virtual Negocio Negocio { get; set; } = null!;

    [InverseProperty("Reserva")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    [InverseProperty("Reserva")]
    
    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Reservas")]
    [JsonIgnore]
    public virtual Usuario Usuario { get; set; } = null!;

    [InverseProperty("Reserva")]
    public virtual ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
}
