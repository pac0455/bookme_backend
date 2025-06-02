namespace bookme_backend.DataAcces.DTO.Pasarela
{
    public class ResultadoPasarela
    {
        public bool Exitoso { get; set; }
        public string IdTransaccion { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string CodigoRespuesta { get; set; } = null!;
        public string DetallesRespuesta { get; set; } = null!;
    }
}
