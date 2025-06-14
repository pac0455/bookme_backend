namespace bookme_backend.DataAcces.DTO.Usuario
{
    public class UsuarioAdminDTO
    {
        public string Id { get; set; } = null!;           // Id del usuario
        public string Email { get; set; } = null!;        // Email del usuario

        // Resumen general
        public int NegociosActivos { get; set; }
        public int NegociosBloqueados { get; set; }
        public int TotalUsuarios { get; set; }
        public int ReservasUltimos30Dias { get; set; }
        public int TotalValoraciones { get; set; }
        public double PromedioPuntuacion { get; set; }

        // Lista de negocios donde el usuario es admin
        public int NumeroNegocios { get; set; }
    }
}
