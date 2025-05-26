public class UsuarioReservaEstadisticaDto
{
    public string UsuarioId { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public int TotalReservas { get; set; }
    public decimal TotalGastado { get; set; }
    public DateTime? FechaPrimeraReserva { get; set; }
    public DateTime? FechaUltimaReserva { get; set; }  
    public double? FrecuenciaPromedioDias { get; set; } 
    public int TotalCanceladas { get; set; }
    public double? PuntuacionPromedio { get; set; }
    public List<string> ServiciosMasUsados { get; set; }
    public bool EstaSuscrito { get; set; }
}
