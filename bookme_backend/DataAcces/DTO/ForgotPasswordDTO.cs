using System.ComponentModel.DataAnnotations;

namespace bookme_backend.DataAcces.DTO
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}