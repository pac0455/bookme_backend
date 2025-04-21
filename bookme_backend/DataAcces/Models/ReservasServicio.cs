using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Models;

[Table("reservas_servicios")]
public partial class ReservasServicio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("reserva_id")]
    public int ReservaId { get; set; }

    [Column("servicio_id")]
    public int ServicioId { get; set; }

    [ForeignKey("ReservaId")]
    [InverseProperty("ReservasServicios")]
    public virtual Reserva Reserva { get; set; } = null!;

    [ForeignKey("ServicioId")]
    [InverseProperty("ReservasServicios")]
    public virtual Servicio Servicio { get; set; } = null!;
}
