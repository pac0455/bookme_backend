using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace bookme_backend.DataAcces.Models
{
    [Table("RolesGlobales")]
    public class RolGlobal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public ERol Rol { get; set; } 
    }
}
