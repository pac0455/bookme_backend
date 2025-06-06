namespace bookme_backend.DataAcces.DTO.Usuario
{
    public class UpdatePasswordDTO
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
