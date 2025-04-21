using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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
    public decimal? Monto { get; set; }

    [Column("fecha_pago", TypeName = "datetime")]
    public DateTime? FechaPago { get; set; }

    [Column("estado_pago")]
    [StringLength(255)]
    public string EstadoPago { get; set; } = null!;

    [Column("metodo_pago")]
    [StringLength(255)]
    public string MetodoPago { get; set; } = null!;

    [Column("id_transaccion_externa")]
    [StringLength(255)]
    public string? IdTransaccionExterna { get; set; }

    [Column("respuesta_pasarela")]
    public string? RespuestaPasarela { get; set; }

    [Column("moneda")]
    [StringLength(10)]
    public string? Moneda { get; set; }

    [Column("reembolsado")]
    public bool? Reembolsado { get; set; }

    [Column("creado", TypeName = "datetime")]
    public DateTime? Creado { get; set; }

    [Column("actualizado", TypeName = "datetime")]
    public DateTime? Actualizado { get; set; }

    [ForeignKey("ReservaId")]
    [InverseProperty("Pagos")]
    public virtual Reserva Reserva { get; set; } = null!;
}
