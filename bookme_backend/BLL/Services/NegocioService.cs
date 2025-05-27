using System.Net.Http;
using System.Text.Json;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace bookme_backend.BLL.Services
{
    public class NegocioService(
        IRepository<Negocio> negocioRepo,
        IRepository<Suscripcion> subcripcionesRepo,
        IRepository<Reserva> reservaRepo,
        IRepository<Servicio> servicioRepo,
        ILogger<NegocioService> logger,
        IOptions<GoogleMapsSettings> googleMapsOptions,
        HttpClient httpClient


    ) : INegocioService
    {
        private readonly IRepository<Negocio> _negocioRepo = negocioRepo;
        private readonly HttpClient _httpClient= httpClient;
        private readonly IRepository<Suscripcion> _subcripcionesRepo = subcripcionesRepo;
        private readonly IRepository<Reserva> _reservaRepo = reservaRepo;
        private readonly IRepository<Servicio> _servicioRepo = servicioRepo;
        private readonly ILogger<NegocioService> _logger = logger;
        private readonly GoogleMapsSettings _googleMapsSettings = googleMapsOptions.Value;



        //Crea un negocio y lo relaciona con el usuario mediante una subcripción
        public async Task<(bool Success, string Message)> AddAsync(Negocio negocio, string usuarioId)
        {
            try
            {
                await _negocioRepo.AddAsync(negocio);
                await _negocioRepo.SaveChangesAsync();

                _logger.LogInformation($"Total horarios recibidos: {negocio.HorariosAtencion?.Count()}");

                var suscripcion = new Suscripcion
                {
                    FechaSuscripcion = DateTime.Now,
                    IdNegocio = negocio.Id,
                    IdUsuario = usuarioId,
                    RolNegocio = ERol.NEGOCIO.ToString(),
                };
                await _subcripcionesRepo.AddAsync(suscripcion);
                await _subcripcionesRepo.SaveChangesAsync();

                return (true, "Negocio añadido correctamente.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" Inner Exception: {ex.InnerException.Message}" : string.Empty;
                return (false, $"Error al insertar negocio: {ex.Message}{innerMessage}");
            }
        }
        public async Task<(bool Success, string Message)> UpdateByNombreAsync(Negocio negocio, string usuarioId)
        {
            // Buscar el negocio por nombre
            var negocioActual = await _negocioRepo
                .GetWhereAsync(n => n.Nombre.ToLower() == negocio.Nombre.ToLower());

            var actual = negocioActual.FirstOrDefault();
            if (actual == null)
                return (false, "Negocio no encontrado por nombre.");

            // Asignamos el ID al objeto que recibimos, para usar UpdateAsync()
            negocio.Id = actual.Id;

            // Reutilizamos toda la lógica ya validada
            return await UpdateAsync(negocio, usuarioId);
        }
        public async Task<(bool Success, string Message, List<ReservaDetalladaDto> Reservas)> GetReservasDetalladasByNegocioIdAsync(int negocioId)
        {
            try
            {
                // Verificar si el negocio existe
                var negocioExiste = await _negocioRepo.Exist(n => n.Id == negocioId);

                if (!negocioExiste)
                {
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<ReservaDetalladaDto>());
                }

                // Traer los datos con Includes anidados
                var reservas = await _reservaRepo.GetWhereAsync(
                    r => r.NegocioId == negocioId,
                    q => q
                        .Include(r => r.ReservasServicios).ThenInclude(rs => rs.Servicio)
                        .Include(r => r.Pagos)
                );


                // Mapear las reservas a DTO
                var reservasDetalladas = reservas.Select(r => new ReservaDetalladaDto
                {
                    ReservaId = r.Id,
                    Fecha = r.Fecha,
                    Estado = r.Estado,
                    ComentarioCliente = r.ComentarioCliente,
                    EstadoPagoGeneral = r.Pagos.FirstOrDefault()?.EstadoPago ?? "sin pago",
                    Servicios = r.ReservasServicios.Select(rs => new ServicioConPagoDto
                    {
                        Nombre = rs.Servicio?.Nombre,
                        Precio = (double?)rs.Servicio?.Precio,
                        Pago = r.Pagos.FirstOrDefault() != null
                            ? new PagoDto
                            {
                                Monto = (double)r.Pagos.First().Monto,
                                Estado = r.Pagos.First().EstadoPago,
                                Metodo = r.Pagos.First().MetodoPago,
                                FechaPago = r.Pagos.First().FechaPago ?? DateTime.MinValue
                            }
                            : null
                    }).ToList()
                }).ToList();

                return (true, "Reservas detalladas obtenidas correctamente.", reservasDetalladas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reservas detalladas del negocio con ID {negocioId}");
                return (false, $"Error al obtener las reservas detalladas: {ex.Message}", new List<ReservaDetalladaDto>());
            }
        }

        public async Task<(bool Success, string Message, Negocio? Negocio)> UpdateNegocioImagenAsync(int negocioId, Imagen imagen)
        {

            var nuevaImagen = imagen.Url;
            if (nuevaImagen == null || nuevaImagen.Length == 0)
                return (false, "La imagen no es válida.", null);

            var negocio = await _negocioRepo.GetByIdAsync(negocioId);
            if (negocio == null)
                return (false, $"No se encontró el negocio con ID {negocioId}.", null);

            try
            {
                // Eliminar imagen anterior si existe
                if (!string.IsNullOrWhiteSpace(negocio.LogoUrl))
                {
                    var rutaAnterior = Path.Combine(Directory.GetCurrentDirectory(), negocio.LogoUrl.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(rutaAnterior))
                        File.Delete(rutaAnterior);
                }

                // Guardar nueva imagen
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "negocios");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{nuevaImagen.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await nuevaImagen.CopyToAsync(stream);
                }

                negocio.LogoUrl = Path.Combine("uploads", "negocios", uniqueFileName).Replace("\\", "/");

                _negocioRepo.Update(negocio);
                await _negocioRepo.SaveChangesAsync();

                return (true, "Imagen del negocio actualizada correctamente.", negocio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la imagen del negocio.");
                return (false, $"Error al actualizar la imagen del negocio: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, byte[]? ImageBytes, string ContentType)> GetNegocioImagenAsync(int negocioId)
        {
            var negocio = await _negocioRepo.GetByIdAsync(negocioId);
            if (negocio == null)
                return (false, "Negocio no encontrado.", null, "");

            if (string.IsNullOrEmpty(negocio.LogoUrl))
                return (false, "El negocio no tiene imagen asignada.", null, "");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), negocio.LogoUrl.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(filePath))
                return (false, "La imagen no existe en el servidor.", null, "");

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return (true, "Imagen obtenida correctamente.", imageBytes, contentType);
        }

        public async Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosReservadosByNegocioIdAsync(int negocioId)
        {
            try
            {
                // Verificamos si el negocio existe
                var negocioExiste = await _negocioRepo.Exist(n => n.Id == negocioId);
                if (!negocioExiste)
                {
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<Servicio>());
                }

                // Cargamos reservas con servicios incluidos
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.ReservasServicios.Select(rs => rs.Servicio)
                );

                // Extraemos todos los servicios reservados
                var serviciosReservados = reservas
                    .SelectMany(r => r.ReservasServicios)
                    .Select(rs => rs.Servicio)
                    .Where(s => s != null) // por seguridad
                    .DistinctBy(s => s.Id) // evitar duplicados
                    .ToList();

                return (true, "Servicios reservados obtenidos correctamente.", serviciosReservados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener servicios reservados del negocio con ID {negocioId}");
                return (false, $"Error al obtener los servicios reservados: {ex.Message}", new List<Servicio>());
            }
        }

        public async Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosByNegocioIdAsync(int negocioId)
        {
            try
            {
                // Verificar si el negocio existe
                var negocioExiste = await _negocioRepo.Exist(n => n.Id == negocioId);

                if (!negocioExiste)
                {
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<Servicio>());
                }

                var servicios = await _servicioRepo.GetWhereAsync(s => s.NegocioId == negocioId);

                return (true, "Servicios obtenidos correctamente.", servicios.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener servicios del negocio con ID {negocioId}");
                return (false, $"Error al obtener los servicios: {ex.Message}", new List<Servicio>());
            }
        }


        public async Task<(bool Success, string Message)> AddRangeAsync(List<Negocio> negocios, string usuarioId)
        {
            try
            {
                foreach (var negocio in negocios)
                {
                    await _negocioRepo.AddAsync(negocio);
                }
                return (true, "Negocios añadidos correctamente.");
            }
            catch (Exception ex) {

                return (false, $"Error al insertar horarios: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, List<Negocio> negocios)> GetByUserId(string userId)
        {
            try
            {
                var suscripciones = await _subcripcionesRepo
                    .GetWhereAsync(s => s.IdUsuario == userId && s.RolNegocio == ERol.NEGOCIO.ToString());

                var negocioIds = suscripciones.Select(s => s.IdNegocio).Distinct().ToList();

                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => negocioIds.Contains(n.Id),
                    n => n.HorariosAtencion,  // Incluir horarios,
                    n => n.Categoria
                );


                return (true, "Negocios obtenidos correctamente", negocios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener los negocios del usuario con ID: {userId}");
                return (false, $"Error al obtener los negocios: {ex.Message}", new List<Negocio>());
            }
        }
        public async Task<(bool Success, string Message)> UpdateAsync(Negocio negocio, string usuarioId)
        {
            var negocioActual = await _negocioRepo.GetByIdAsync(negocio.Id);
            if (negocioActual == null)
                return (false, "Negocio no encontrado.");

            // Validar que el negocio pertenece al usuario
            var suscripciones = await _subcripcionesRepo.GetWhereAsync(s => s.IdUsuario == usuarioId && s.IdNegocio == negocio.Id);
            if (!suscripciones.Any())
                return (false, "No tienes permisos para modificar este negocio.");

            // Validar duplicado solo si el nombre ha cambiado
            if (!string.Equals(negocioActual.Nombre, negocio.Nombre, StringComparison.Ordinal))
            {
                var yaExiste = await _negocioRepo.Exist(n => n.Nombre == negocio.Nombre && n.Id != negocio.Id);
                if (yaExiste)
                    return (false, "Ya existe otro negocio con ese nombre.");

                negocioActual.Nombre = negocio.Nombre;
            }

            // Actualizar solo si cambian
            if (!string.Equals(negocioActual.Descripcion, negocio.Descripcion, StringComparison.Ordinal))
                negocioActual.Descripcion = negocio.Descripcion;

            if (!string.Equals(negocioActual.Direccion, negocio.Direccion, StringComparison.Ordinal))
                negocioActual.Direccion = negocio.Direccion;

            if (negocioActual.Latitud != negocio.Latitud)
                negocioActual.Latitud = negocio.Latitud;

            if (negocioActual.Longitud != negocio.Longitud)
                negocioActual.Longitud = negocio.Longitud;

            if (!string.Equals(negocioActual.Categoria.Nombre, negocio.Categoria.Nombre, StringComparison.Ordinal))
                negocioActual.Categoria = negocio.Categoria;

            _negocioRepo.Update(negocioActual);
            await _negocioRepo.SaveChangesAsync();

            return (true, "Negocio actualizado correctamente.");
        }

        public async Task<(bool Success, string Message, List<Reserva> Reservas)> GetReservasByNegocioIdAsync(int negocioId)
        {
            try
            {
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.Usuario,
                    r => r.ReservasServicios,
                    r => r.Pagos,
                    r => r.Valoraciones
                );

                return (true, "Reservas obtenidas correctamente.", reservas.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reservas del negocio con ID {negocioId}");
                return (false, $"Error al obtener las reservas: {ex.Message}", new List<Reserva>());
            }
        }
        public async Task<(bool Success, string Message, List<NegocioCardCliente> Negocios)> GetNegociosParaClienteAsync(Ubicacion? ubicacionUser)
        {
            try
            {
                // Obtener todos los negocios activos con categoría y valoraciones
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => n.Activo ?? false,
                    n => n.Categoria,
                    n => n.ResenasNegocio
                );
                var negociosDto = new List<NegocioCardCliente>();

                // Proyectar a DTO incluyendo cálculo de distancia si la ubicación del usuario existe
                foreach (var n in negocios)
                {
                    var distancia = await CalcularDistanciaConGoogleAsync(
                        ubicacionUser,
                        new Ubicacion { Latitud = n.Latitud, Longitud = n.Longitud }
                    );
                    negociosDto.Add(new NegocioCardCliente
                    {
                        Id = n.Id,
                        Nombre = n.Nombre,
                        Descripcion = n.Descripcion,
                        Categoria = n.Categoria?.Nombre ?? "Sin categoría",
                        Direccion = n.Direccion,
                        Rating = n.ResenasNegocio.Any() ? (float)n.ResenasNegocio.Average(r => r.Puntuacion) : 0f,
                        ReviewCount = n.ResenasNegocio.Count,
                        IsActive = n.Activo ?? false,
                        IsOpen = EstaAbierto(n.HorariosAtencion),
                        Distancia = distancia,
                        Latitud = n.Latitud,
                        Longitud = n.Longitud
                    });

                }



                return (true, "Negocios obtenidos correctamente", negociosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener negocios para cliente");
                return (false, $"Error al obtener negocios: {ex.Message}", new List<NegocioCardCliente>());
            }
        }
        public async Task<double?> CalcularDistanciaConGoogleAsync(Ubicacion origen, Ubicacion destino)
        {
            if (origen?.Latitud == null || origen?.Longitud == null ||
                destino?.Latitud == null || destino?.Longitud == null)
                return null;

            string url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                $"?origins={origen.Latitud},{origen.Longitud}" +
                $"&destinations={destino.Latitud},{destino.Longitud}" +
                $"&mode=driving" +
                $"&key={_googleMapsSettings.ApiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;

            var status = root.GetProperty("status").GetString();
            if (status != "OK") return null;

            var element = root.GetProperty("rows")[0].GetProperty("elements")[0];
            if (element.GetProperty("status").GetString() != "OK") return null;

            int metros = element.GetProperty("distance").GetProperty("value").GetInt32();
            return Math.Round(metros / 1000.0, 2); // en kilómetros
        }



        // Ejemplo simple de método para saber si negocio está abierto según horarios
        private bool EstaAbierto(ICollection<Horario> horarios)
        {
            var diaSemanaActual = DateTime.Now.DayOfWeek.ToString(); // Ej: "Monday"

            // Mapea "Monday" -> "Lunes" (porque en la DB estás usando español)
            var diaSemanaMap = new Dictionary<string, string>
            {
                ["Monday"] = "Lunes",
                ["Tuesday"] = "Martes",
                ["Wednesday"] = "Miércoles",
                ["Thursday"] = "Jueves",
                ["Friday"] = "Viernes",
                ["Saturday"] = "Sábado",
                ["Sunday"] = "Domingo"
            };

            if (!diaSemanaMap.TryGetValue(diaSemanaActual, out var diaSemanaDb))
                return false;

            var ahora = DateTime.Now.TimeOfDay;

            return horarios
                .Where(h => h.DiaSemana.Equals(diaSemanaDb, StringComparison.OrdinalIgnoreCase))
                .Any(h => ahora >= h.HoraInicio && ahora <= h.HoraFin);
        }



    }
}
