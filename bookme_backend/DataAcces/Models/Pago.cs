using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using bookme_backend.DataAcces.DTO.Pago;

namespace bookme_backend.DataAcces.Models;

[Table("pagos")]
public partial class Pago
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("reserva_id")]
    public int ReservaId { get; set; }

    [Column("monto", TypeName = "decimal(10, 2)")]
    public decimal Monto { get; set; }

    [Column("fecha_pago", TypeName = "datetime")]
    public DateTime? FechaPago { get; set; }

    [Column("estado_pago")]
    public EstadoPago EstadoPago { get; set; }

    [Column("metodo_pago")]
    public MetodoPago MetodoPago { get; set; }

    [Column("respuesta_pasarela")]
    public string? RespuestaPasarela { get; set; }

    [Column("moneda")]
    [StringLength(10)]
    public string? Moneda { get; set; }

    [ForeignKey("ReservaId")]
    [InverseProperty("Pago")]
    public virtual Reserva Reserva { get; set; } = null!;
}
