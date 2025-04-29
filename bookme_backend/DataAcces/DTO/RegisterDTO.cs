namespace bookme_backend.DataAcces.DTO
{
    public class RegisterDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? FirebaseUid { get; set; }

    }
}
