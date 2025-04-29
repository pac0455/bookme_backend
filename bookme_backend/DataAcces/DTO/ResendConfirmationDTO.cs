using System.ComponentModel.DataAnnotations;

namespace bookme_backend.DataAcces.DTO
{
    public class ResendConfirmationDTO
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }
    }
}
