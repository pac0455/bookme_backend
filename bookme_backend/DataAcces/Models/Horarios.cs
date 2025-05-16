using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bookme_backend.DataAcces.Models
{
    [Table("horarios")]
    public class Horarios
    {
        [Key]
        [Column]
        public int Id { get; set; }
        [Column("id_negocio")]
        public int IdNegocio { get; set; }

        [Column("dia_semana")]
        [StringLength(20)]
        public string DiaSemana { get; set; } = null!; // Ej: "Lunes", "Martes", etc.

        [Column("hora_inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeSpan HoraFin { get; set; }

        [ForeignKey(nameof(IdNegocio))]
        [InverseProperty("HorariosAtencion")]
        public virtual Negocio Negocio { get; set; } = null!;
    }
}
