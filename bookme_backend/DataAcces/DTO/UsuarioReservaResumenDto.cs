namespace bookme_backend.DataAcces.DTO;
using System;
using System.Collections.Generic;

public class ClienteResumenDto
{
    // Datos básicos
    public string IdUsuario { get; set; }
    public string Email { get; set; }

    // Suscripción
    // public string RolNegocio { get; set; } no hace falta filtro por solo el rol CLIENTE
    public DateTime? FechaSuscripcion { get; set; }

    // Historial de reservas
    public int TotalReservas { get; set; }
    public double FrecuenciaReservasPorMes { get; set; }
    public List<string> DiasPreferidos { get; set; }
    public List<string> HorariosPreferidos { get; set; }
    public Dictionary<string, int> EstadoReservas { get; set; }

    // Servicios preferidos
    public List<ServicioFrecuenteDto> ServiciosFrecuentes { get; set; }

    // Pagos
    public double TotalGastado { get; set; }
    public Dictionary<string, int> MetodosPagoUsados { get; set; }
    public HashSet<string> MonedasUsadas { get; set; }
    public Dictionary<string, int> EstadoPagos { get; set; }

    // Valoraciones
    public double PuntuacionPromedio { get; set; }
    public int TotalValoraciones { get; set; }
    public List<string> ComentariosRecientes { get; set; }

    // Métricas adicionales
    public int UltimaReservaHaceDias { get; set; }
    public double TasaCancelacion { get; set; }
    public Dictionary<string, double> IngresosPorMes { get; set; }
    public List<ServicioFrecuenteDto> ServiciosFavoritosPorGasto { get; set; }
}

public class ServicioFrecuenteDto
{
    public int IdServicio { get; set; }
    public string Nombre { get; set; }
    public int VecesReservado { get; set; }
    public double TotalGastado { get; set; }
    public int Valoraciones { get; set; }
}
