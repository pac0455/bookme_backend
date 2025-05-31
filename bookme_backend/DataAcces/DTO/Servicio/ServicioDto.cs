namespace bookme_backend.DataAcces.DTO.Servicio
{
    public class ServicioDto
    {
        public int NegocioId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }

        public IFormFile? Imagen { get; set; }
    }

}
