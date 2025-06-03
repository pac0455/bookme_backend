namespace bookme_backend.DataAcces.DTO.Usuario
{
    public class RegisterDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool IsNegocio { get; set; }
    }
}
