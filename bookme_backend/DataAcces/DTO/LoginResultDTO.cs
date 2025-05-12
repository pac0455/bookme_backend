using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.DTO
{
    public class LoginResultDTO
    {
        public string Token { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();

    }

}
