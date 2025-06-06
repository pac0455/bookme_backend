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
    }
}
