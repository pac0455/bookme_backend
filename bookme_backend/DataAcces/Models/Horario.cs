using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bookme_backend.DataAcces.Models
{
    [Table("horarios")]
    public class Horario
    {
        [Key]
        [Column]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_negocio")]
        public int IdNegocio { get; set; }

        [Column("dia_semana")]
        [StringLength(20)]
        public string DiaSemana { get; set; } = null!;

        [Column("hora_inicio")]
        public TimeOnly HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeOnly HoraFin { get; set; }

        [ForeignKey(nameof(IdNegocio))]
        [JsonIgnore]
        [InverseProperty("HorariosAtencion")]
        public virtual Negocio? Negocio { get; set; }
    }

}
