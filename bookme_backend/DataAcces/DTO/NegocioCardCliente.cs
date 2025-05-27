namespace bookme_backend.DataAcces.DTO
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
}
