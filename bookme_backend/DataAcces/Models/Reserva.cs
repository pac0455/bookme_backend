using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    public DateOnly Fecha { get; set; }

    [Column("hora_inicio")]
    public TimeOnly HoraInicio { get; set; }

    [Column("hora_fin")]
    public TimeOnly HoraFin { get; set; }

    [Column("estado")]
    [StringLength(255)]
    public EstadoReserva Estado { get; set; }

    [Column("fecha_creacion", TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [Column("servicio_id")]
    public int ServicioId { get; set; }

    [ForeignKey("NegocioId")]
    [InverseProperty("Reservas")]
    [JsonIgnore]
    public virtual Negocio Negocio { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Reservas")]
    [JsonIgnore]
    public virtual Usuario Usuario { get; set; } = null!;

    [ForeignKey("ServicioId")]
    [InverseProperty("Reservas")]
    public virtual Servicio Servicio { get; set; } = null!;

    [InverseProperty("Reserva")]
    public virtual Pago Pago { get; set; }
}
