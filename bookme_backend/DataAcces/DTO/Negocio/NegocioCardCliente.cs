namespace bookme_backend.DataAcces.DTO.NegocioDTO
{
    public class NegocioCardCliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public float Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsOpen { get; set; }

        public double? Distancia { get; set; } // en kilómetros y andando
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
    }
    public static class NegocioMapper
    {
        public static NegocioCardCliente ToNegocioCardCliente(Models.Negocio negocio, double? distancia, bool isOpen)
        {
            return new NegocioCardCliente
            {
                Id = negocio.Id,
                Nombre = negocio.Nombre,
                Descripcion = negocio.Descripcion,
                Categoria = negocio.Categoria?.Nombre ?? "Sin categoría",
                Direccion = negocio.Direccion,
                Rating = negocio.Valoraciones.Any() ? (float)negocio.Valoraciones.Average(r => r.Puntuacion) : 0f,
                ReviewCount = negocio.Valoraciones.Count,
                IsActive = negocio.Activo,
                IsOpen = isOpen,
                Distancia = distancia,
                Latitud = negocio.Latitud,
                Longitud = negocio.Longitud
            };
        }
    }

}
