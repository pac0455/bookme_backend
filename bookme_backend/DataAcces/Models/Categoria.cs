using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bookme_backend.DataAcces.Models;

[Table("categoria")]
public class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("categoria")]
    [Required, StringLength(100)]
    public string Nombre { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Negocio> Negocios { get; set; } = new List<Negocio>();
}