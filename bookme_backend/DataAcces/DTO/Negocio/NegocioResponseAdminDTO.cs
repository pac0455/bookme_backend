using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.DTO.NegocioDTO
{
    public class GetAllNegociosResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? InnerMessage { get; set; } //Mensaje para detalles técnicos
        public string? ErrorCode { get; set; }
        public List<NegocioResponseAdminDTO>? Data { get; set; }
    }
    public class NegocioResponseAdminDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public CategoriaDTo Categoria { get; set; }
        public string Direccion { get; set; } = null!;
        public float Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsOpen { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public bool Bloqueado { get; set; }
    }
    public class CategoriaDTo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }


    public static class NegocioAdminMapper
        {
            // Ahora recibe un Func para determinar si está abierto
            public static NegocioResponseAdminDTO ToNegocioResponseAdminDTO(
                Negocio negocio,
                bool estaAbierto)
            {
                return new NegocioResponseAdminDTO
                {
                    Id = negocio.Id,
                    Nombre = negocio.Nombre,
                    Descripcion = negocio.Descripcion ?? "Sin descripción",
                    Categoria = new CategoriaDTo
                    {
                        Id = negocio.Categoria?.Id ?? 0,
                        Nombre = negocio.Categoria?.Nombre ?? "Sin categoría"
                    },
                    Direccion = negocio.Direccion,
                    Rating = negocio.Valoraciones.Any() ? (float)negocio.Valoraciones.Average(v => v.Puntuacion) : 0f,
                    ReviewCount = negocio.Valoraciones.Count,
                    IsActive = negocio.Activo,
                    IsOpen = estaAbierto,
                    Latitud = negocio.Latitud,
                    Longitud = negocio.Longitud,
                    Bloqueado = negocio.Bloqueado
                };
            }
        }

}
