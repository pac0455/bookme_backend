namespace bookme_backend.DataAcces.DTO
{
    public class UsuarioRegistroDto
    {
        public string FirebaseUid { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }

    }

}
