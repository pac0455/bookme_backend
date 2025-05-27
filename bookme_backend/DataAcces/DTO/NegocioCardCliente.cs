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
        public int Distancia { get; set; }  // Si la calculas en backend o desde el cliente, esto es opcional
    }
}
