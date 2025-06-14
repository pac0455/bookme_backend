namespace bookme_backend.DataAcces.DTO.Servicio
{
    public class ServicioDetalleDto
    {
        public int Id { get; set; }
        public int NegocioId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? DuracionMinutos { get; set; }
        public decimal? Precio { get; set; }

        public string NegocioNombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = "Sin categoría";

        // Valoraciones agregadas del negocio
        public double ValoracionPromedioNegocio { get; set; }
        public int NumeroValoracionesNegocio { get; set; }

        public int NumeroReservas { get; set; }
        public string? ImagenUrl { get; set; }

        public static ServicioDetalleDto FromServicio(
            Models.Servicio servicio,
            Models.Negocio negocio,
            double valoracionPromedioNegocio,
            int numeroValoracionesNegocio,
            int numeroReservas)
        {
            return new ServicioDetalleDto
            {
                Id = servicio.Id,
                NegocioId = servicio.NegocioId,
                Nombre = servicio.Nombre ?? string.Empty,
                Descripcion = servicio.Descripcion ?? string.Empty,
                DuracionMinutos = servicio.DuracionMinutos,
                Precio = servicio.Precio,
                ImagenUrl = servicio.ImagenUrl,

                NegocioNombre = negocio.Nombre ?? string.Empty,
                Categoria = negocio.Categoria?.Nombre ?? "Sin categoría",

                ValoracionPromedioNegocio = valoracionPromedioNegocio,
                NumeroValoracionesNegocio = numeroValoracionesNegocio,
                NumeroReservas = numeroReservas
            };
        }

    }
}
