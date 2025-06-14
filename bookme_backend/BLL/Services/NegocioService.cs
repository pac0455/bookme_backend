using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO;
using bookme_backend.DataAcces.DTO.Google;
using bookme_backend.DataAcces.DTO.NegocioDTO;
using bookme_backend.DataAcces.DTO.Pago;
using bookme_backend.DataAcces.DTO.Reserva;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace bookme_backend.BLL.Services
{
    public class NegocioService(
        IRepository<Negocio> negocioRepo,
        IRepository<Suscripcion> subcripcionesRepo,
        IRepository<Reserva> reservaRepo,
        IRepository<Servicio> servicioRepo,
        IRepository<Horario> horarioRepo,
        IRepository<Pago> pagoRepo,


        ILogger<NegocioService> logger,
        IOptions<GoogleMapsSettings> googleMapsOptions,
        HttpClient httpClient


    ) : INegocioService
    {
        private readonly IRepository<Negocio> _negocioRepo = negocioRepo;
        private readonly IRepository<Horario> _horarioRepo = horarioRepo;
        private readonly IRepository<Pago> _pagoRepo = pagoRepo;


        private readonly HttpClient _httpClient = httpClient;
        private readonly IRepository<Suscripcion> _subcripcionesRepo = subcripcionesRepo;
        private readonly IRepository<Reserva> _reservaRepo = reservaRepo;
        private readonly IRepository<Servicio> _servicioRepo = servicioRepo;
        private readonly ILogger<NegocioService> _logger = logger;
        private readonly GoogleMapsSettings _googleMapsSettings = googleMapsOptions.Value;

        //Fucion auxiliar para sacar negocio





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
            catch (Exception ex)
            {

                return (false, $"Error al insertar horarios: {ex.Message}");
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

                // Cargamos reservas con servicio incluido
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.Servicio
                );

                // Extraemos todos los servicios reservados (único por reserva)
                var serviciosReservados = reservas
                    .Select(r => r.Servicio)
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
        public async Task<(bool Success, string Message, List<Negocio> negocios)> GetNegociosByUserId(string userId)
        {
            try
            {
                var suscripciones = await _subcripcionesRepo
                    .GetWhereAsync(s => s.IdUsuario == userId && s.RolNegocio == ERol.NEGOCIO.ToString());

                var negocioIds = suscripciones.Select(s => s.IdNegocio).Distinct().ToList();

                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => negocioIds.Contains(n.Id) && !n.Eliminado,
                    n => n.HorariosAtencion,
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
            _logger.LogInformation("UpdateAsync recibido con negocio: {@Negocio}", negocio);

            var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                x => x.Id == negocio.Id,
                n => n.Categoria,
                n => n.HorariosAtencion
            );

            var negocioActual = negocios.FirstOrDefault();
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

            // Actualizar campos si cambian
            if (!string.Equals(negocioActual.Descripcion, negocio.Descripcion, StringComparison.Ordinal))
                negocioActual.Descripcion = negocio.Descripcion;

            if (!string.Equals(negocioActual.Direccion, negocio.Direccion, StringComparison.Ordinal))
                negocioActual.Direccion = negocio.Direccion;

            if (negocioActual.Latitud != negocio.Latitud)
                negocioActual.Latitud = negocio.Latitud;

            if (negocioActual.Longitud != negocio.Longitud)
                negocioActual.Longitud = negocio.Longitud;

            if (negocioActual.CategoriaId != negocio.CategoriaId)
                negocioActual.CategoriaId = negocio.CategoriaId;

            if (negocioActual.Activo != negocio.Activo)
                negocioActual.Activo = negocio.Activo;

            // --- ACTUALIZAR HORARIOS ---
            var horariosActuales = negocioActual.HorariosAtencion.ToList();

            // Eliminar horarios que ya no están
            foreach (var horarioActual in horariosActuales)
            {
                if (!negocio.HorariosAtencion.Any(h => h.Id == horarioActual.Id))
                {
                    await _horarioRepo.DeleteAsync(horarioActual);
                }
            }

            // Agregar o actualizar horarios
            foreach (var horario in negocio.HorariosAtencion)
            {
                var existente = horariosActuales.FirstOrDefault(h => h.Id == horario.Id);
                if (existente != null)
                {
                    existente.DiaSemana = horario.DiaSemana;
                    existente.HoraInicio = horario.HoraInicio;
                    existente.HoraFin = horario.HoraFin;
                }
                else
                {
                    horario.IdNegocio = negocio.Id;
                    negocioActual.HorariosAtencion.Add(horario);
                }
            }

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
                    r => r.Pago
                );

                return (true, "Reservas obtenidas correctamente.", reservas.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reservas del negocio con ID {negocioId}");
                return (false, $"Error al obtener las reservas: {ex.Message}", new List<Reserva>());
            }
        }

        //Funcion que se usará para mostrar al cliente los negocios activos
        public async Task<(bool Success, string Message, List<NegocioCardCliente> Negocios)> GetNegociosParaClienteAsync(Ubicacion? ubicacionUser)
        {
            try
            {
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => n.Activo && !n.Eliminado,
                    n => n.Categoria,
                    n => n.HorariosAtencion,
                    n => n.Valoraciones
                );

                if (negocios == null || !negocios.Any())
                    return (true, "No se encontraron negocios.", new List<NegocioCardCliente>());

                var negociosDto = new List<NegocioCardCliente>();

                foreach (var n in negocios)
                {
                    double? distancia = null;
                    if (ubicacionUser != null)
                    {
                        distancia = await CalcularDistanciaConGoogleAsync(
                            ubicacionUser,
                            new Ubicacion { Latitud = n.Latitud, Longitud = n.Longitud }
                        );
                    }

                    bool isOpen = EstaAbierto(n.HorariosAtencion);

                    var dto = NegocioMapper.ToNegocioCardCliente(n, distancia, isOpen);

                    negociosDto.Add(dto);
                }

                return (true, "Negocios obtenidos correctamente", negociosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener negocios para cliente");
                return (false, $"Error al obtener negocios: {ex.Message}", new List<NegocioCardCliente>());
            }
        }

        public async Task<(bool Success, string Message, NegocioCardCliente? Negocio)> GetNegocioParaClienteAsync(int negocioId, Ubicacion? ubicacionUser)
        {
            try
            {
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => n.Id == negocioId && n.Activo && !n.Eliminado,
                    n => n.Categoria,
                    n => n.HorariosAtencion,
                    n => n.Valoraciones
                );

                var negocio = negocios.FirstOrDefault();

                if (negocio == null)
                    return (false, "Negocio no encontrado.", null);

                double? distancia = null;
                if (ubicacionUser != null && negocio.Latitud.HasValue && negocio.Longitud.HasValue)
                {
                    distancia = await CalcularDistanciaConGoogleAsync(
                        ubicacionUser,
                        new Ubicacion { Latitud = negocio.Latitud, Longitud = negocio.Longitud }
                    );
                }

                bool isOpen = EstaAbierto(negocio.HorariosAtencion);

                var dto = NegocioMapper.ToNegocioCardCliente(negocio, distancia, isOpen);

                return (true, "Negocio obtenido correctamente", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener negocio para cliente");
                return (false, $"Error al obtener negocio: {ex.Message}", null);
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
        // Método para saber si el negocio está abierto según horarios
        private bool EstaAbierto(ICollection<Horario> horarios)
        {
            var diaSemanaActual = DateTime.Now.DayOfWeek.ToString();

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

            string Normalizar(string texto) =>
                texto.Normalize(System.Text.NormalizationForm.FormD)
                     .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                     .Aggregate("", (s, c) => s + c)
                     .ToLower();

            var diaNormalizado = Normalizar(diaSemanaDb);
            var ahora = DateTime.Now.TimeOfDay;
            _logger.LogDebug("Hora actual: {HoraActual}", ahora);

            foreach (var h in horarios.Where(h => Normalizar(h.DiaSemana) == diaNormalizado))
            {
                _logger.LogDebug("Horario DB - Día: {Dia}, HoraInicio: {Inicio}, HoraFin: {Fin}",
                    h.DiaSemana, h.HoraInicio, h.HoraFin);

                var inicio = new TimeSpan(h.HoraInicio.Hour, h.HoraInicio.Minute, h.HoraInicio.Second);
                var fin = new TimeSpan(h.HoraFin.Hour, h.HoraFin.Minute, h.HoraFin.Second);

                _logger.LogDebug("Horario convertido a TimeSpan - Inicio: {InicioTS}, Fin: {FinTS}", inicio, fin);
                _logger.LogDebug("Comparando hora actual {Ahora} con rango {Inicio} - {Fin}", ahora, inicio, fin);
                if (ahora >= inicio && ahora <= fin)
                {
                    _logger.LogDebug("Horario está abierto");
                }
                else
                {
                    _logger.LogDebug("Horario cerrado");
                }
            }

            var abiertos = horarios
                .Where(h => Normalizar(h.DiaSemana) == diaNormalizado)
                .Any(h =>
                {
                    var inicio = new TimeSpan(h.HoraInicio.Hour, h.HoraInicio.Minute, h.HoraInicio.Second);
                    var fin = new TimeSpan(h.HoraFin.Hour, h.HoraFin.Minute, h.HoraFin.Second);
                    return ahora >= inicio && ahora <= fin;
                });

            _logger.LogDebug("Comparando día normalizado: {DiaNormalizado}", diaNormalizado);
            foreach (var h in horarios)
            {
                _logger.LogDebug("Horario DB - Día: {Dia}, HoraInicio: {Inicio}, HoraFin: {Fin}",
                    h.DiaSemana, h.HoraInicio, h.HoraFin);
            }
            _logger.LogDebug("¿Negocio abierto (tras comparar días normalizados y horas)?: {Abierto}", abiertos);

            return abiertos;
        }

        // Métod usado por el panel de administración para obtener todos los negocios
        public async Task<(bool Success, string Message, List<NegocioResponseAdminDTO> negocios)> GetAllNegocios()
        {
            try
            {
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => !n.Eliminado,
                    n => n.Categoria,
                    n => n.HorariosAtencion,
                    n => n.Valoraciones
                );

                if (negocios == null || !negocios.Any())
                {
                    return (false, "No se encontraron negocios", new List<NegocioResponseAdminDTO>());
                }

                // Pasar el método EstaAbierto como Func al mapper
              
               
                var negociosDto = negocios
                    .Select(n => NegocioAdminMapper.ToNegocioResponseAdminDTO(n, EstaAbierto(n.HorariosAtencion)))
                    .ToList();

                return (true, "Negocios obtenidos correctamente", negociosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los negocios");
                return (false, $"Error al obtener negocios: {ex.Message}", new List<NegocioResponseAdminDTO>());
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int negocioId, string usuarioId, string motivo)
        {
            var negocio = await _negocioRepo.GetByIdAsync(negocioId);

            if (negocio == null)
                return (false, "Negocio no encontrado.");

            negocio.Activo = false;
            negocio.Eliminado = true;
            _negocioRepo.Update(negocio);
            await _negocioRepo.SaveChangesAsync();

            var reservasCanceladas = await CancelarReservasDeNegocio(negocioId, motivo);
            await CancelarPagosDeNegocio(negocioId, motivo);

            return (true, "Negocio eliminado correctamente.");
        }
        public async Task<(bool Success, string Message)> BloquearNegocioAsync(int negocioId, string usuarioId)
        {
            var negocios = await _negocioRepo.GetWhereAsync(n => n.Id == negocioId && !n.Eliminado);
            var negocio = negocios.FirstOrDefault();

            if (negocio == null)
                return (false, "Negocio no encontrado.");

            if (negocio.Bloqueado)
                return (false, "El negocio ya está bloqueado.");

            negocio.Bloqueado = true;
            negocio.Activo = false;

            _negocioRepo.Update(negocio);
            await _negocioRepo.SaveChangesAsync();

            var reservasCanceladas = await CancelarReservasDeNegocio(negocioId, "Negocio bloqueado por administrador");
            await CancelarPagosDeNegocio(negocioId, "Negocio bloqueado por administrador");

            return (true, "Negocio bloqueado correctamente.");
        }

        public async Task<(bool Success, string Message)> DesbloquearNegocioAsync(int negocioId, string usuarioId)
        {
            var negocios = await _negocioRepo.GetWhereAsync(n => n.Id == negocioId && !n.Eliminado);
            var negocio = negocios.FirstOrDefault();

            if (negocio == null)
                return (false, "Negocio no encontrado.");

            if (!negocio.Bloqueado)
                return (false, "El negocio ya está activo.");

            negocio.Bloqueado = false;
            negocio.Activo = true;

            _negocioRepo.Update(negocio);
            await _negocioRepo.SaveChangesAsync();

            return (true, "Negocio desbloqueado correctamente.");
        }



        // Método para cancelar reservas de un negocio (sin llamada a cancelar pagos aquí)
        private async Task<List<Reserva>> CancelarReservasDeNegocio(int negocioId, string motivo)
        {
            var reservas = (await _reservaRepo.GetWhereAsync(r => r.NegocioId == negocioId && r.Estado != EstadoReserva.Cancelada)).ToList();

            foreach (var reserva in reservas)
            {
                reserva.Estado = EstadoReserva.Cancelada;
                reserva.CancelacionMotivo = motivo;
                _reservaRepo.Update(reserva);
            }

            await _reservaRepo.SaveChangesAsync();

            return reservas;
        }

        // Método independiente que cancela/reembolsa pagos pendientes de un negocio, sin requerir reservas externas
        private async Task CancelarPagosDeNegocio(int negocioId, string motivo)
        {
            // Obtiene las reservas activas o canceladas que tengan pagos no reembolsados
            var reservasConPagos = (await _reservaRepo.GetWhereWithIncludesAsync(r =>
                r.NegocioId == negocioId && r.Pago != null && r.Pago.EstadoPago != EstadoPago.Reembolsado,
                r => r.Pago
            )).ToList();

            foreach (var reserva in reservasConPagos)
            {
                reserva.Pago.EstadoPago = EstadoPago.Reembolsado;
                // Opcional: podrías agregar motivo de reembolso en pago, si tienes campo para eso
                _pagoRepo.Update(reserva.Pago);
            }

            await _pagoRepo.SaveChangesAsync();
        }
    }
}
