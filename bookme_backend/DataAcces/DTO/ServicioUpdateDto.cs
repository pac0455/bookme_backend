namespace bookme_backend.DataAcces.DTO
{
    public class ServicioUpdateDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public int NegocioId { get; set; }
    }

}
