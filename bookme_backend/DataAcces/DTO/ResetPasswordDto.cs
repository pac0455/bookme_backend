using System.ComponentModel.DataAnnotations;

namespace bookme_backend.DataAcces.DTO
{
    public class ResetPasswordDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}
