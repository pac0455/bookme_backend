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


        // Método estático de conversión
        public Models.Servicio ToServicio(string? imagenUrl = null)
        {
            return new Models.Servicio
            {
                NegocioId = this.NegocioId,
                Nombre = this.Nombre,
                Descripcion = this.Descripcion,
                DuracionMinutos = this.DuracionMinutos,
                Precio = this.Precio,
                ImagenUrl = imagenUrl
            };
        }
    }

}
