using System.Text.Json;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Pasarela;

namespace bookme_backend.BLL.Services
{
    public class PasarelaService : IPasarelaSimulada
    {
        public async Task<ResultadoPasarela> ProcesarPagoAsync(PagoCreateDTO solicitudPago)
        {
            // Simular 1 segundo de procesamiento
            await Task.Delay(1000);

            // Generar ID simple
            var idTransaccion = $"SIM_{DateTime.Now:yyyyMMddHHmmss}";

            // 90% de éxito
            bool exitoso = new Random().Next(1, 11) <= 9;

            return new ResultadoPasarela
            {
                Exitoso = exitoso,
                IdTransaccion = idTransaccion,
                Mensaje = exitoso ? "Pago exitoso" : "Pago fallido"
            };
        }
    }
}