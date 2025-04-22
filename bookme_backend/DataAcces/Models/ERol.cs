using Google.Apis.Util;

namespace bookme_backend.DataAcces.Models
{
    public enum ERol
    {
        [StringValue("cliente")]
        Cliente,
        [StringValue("negocio")]
        Negocio,
        [StringValue("admin")]
        Admin
    }
}
