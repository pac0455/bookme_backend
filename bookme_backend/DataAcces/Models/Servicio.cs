using bookme_backend.DataAcces.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("servicios")]
public partial class Servicio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("negocio_id")]
    public int NegocioId { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string? Nombre { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("duracion_minutos")]
    public int DuracionMinutos { get; set; }

    [Column("precio", TypeName = "decimal(10, 2)")]
    public decimal Precio { get; set; }

    [Column("imagen_url")]
    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    [ForeignKey("NegocioId")]
    [InverseProperty("Servicios")]
    public virtual Negocio Negocio { get; set; } = null!;

    [InverseProperty("Servicio")]
    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}
