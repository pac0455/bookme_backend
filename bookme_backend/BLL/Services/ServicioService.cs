using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using bookme_backend.BLL.Interfaces;
using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.DTO;
using Microsoft.EntityFrameworkCore;
using bookme_backend.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using bookme_backend.DataAcces.DTO.Servicio;


namespace bookme_backend.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IRepository<Negocio> _negocioRepo;
        private readonly IRepository<Reserva> _reservaRepo;
        private readonly IRepository<Servicio> _servicioRepo;
        private readonly ILogger<ServicioService> _logger;

        public ServicioService(
            IRepository<Negocio> negocioRepo,
            IRepository<Reserva> reservaRepo,
            IRepository<Servicio> servicioRepo,
            ILogger<ServicioService> logger)
        {
            _negocioRepo = negocioRepo;
            _reservaRepo = reservaRepo;
            _servicioRepo = servicioRepo;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los servicios que están reservados en un negocio específico.
        /// </summary>
        /// <param name="negocioId">ID del negocio</param>
        /// <returns>Tupla con éxito, mensaje y lista de servicios</returns>
        public async Task<(bool Success, string Message, List<Servicio> Servicios)> GetServiciosByNegocioIdAsync(int negocioId)
        {
            try
            {
                // Verificamos si el negocio existe
                var negocioExiste = await _negocioRepo.Exist(n => n.Id == negocioId);
                if (!negocioExiste)
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<Servicio>());

                // Obtenemos las reservas del negocio, incluyendo el servicio reservado
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.Servicio
                );

                // Extraemos los servicios de todas las reservas, eliminamos duplicados y filtramos nulos
                var serviciosReservados = reservas
                    .Select(r => r.Servicio)     // Obtenemos el servicio directamente de la reserva
                    .Where(s => s != null)       // Filtramos servicios nulos
                    .DistinctBy(s => s.Id)       // Eliminamos duplicados
                    .ToList();

                return (true, "Servicios obtenidos correctamente.", serviciosReservados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener servicios del negocio con ID {negocioId}");
                return (false, $"Error al obtener servicios: {ex.Message}", new List<Servicio>());
            }
        }
        /// <summary>
        /// Obtiene todos los servicios disponibles.
        /// </summary>
        /// <returns>Lista de todos los servicios</returns>
        public async Task<List<Servicio>> GetAllServiciosAsync()
        {
            // Devuelve la lista completa de servicios usando el repositorio genérico
            var negociosActivos = (await _negocioRepo.GetWhereAsync(n => n.Activo && !n.Bloqueado)).Select(n => n.Id).ToList();

            return await _servicioRepo.GetWhereAsync(s => negociosActivos.Contains(s.NegocioId));
        }
        /// <summary>
        /// Obtiene un servicio específico por su ID.
        /// </summary>
        /// <param name="id">ID del servicio</param>
        /// <returns>Servicio encontrado o null</returns>
        public async Task<Servicio> GetServicioByIdAsync(int id)
        {
            // Busca y devuelve el servicio por su clave primaria
            return await _servicioRepo.GetByIdAsync(id);
        }
        /// <summary>
        /// Añade un nuevo servicio a un negocio.
        /// </summary>
        /// <param name="dto">Datos del servicio</param>
        /// <returns>Tupla indicando éxito, mensaje y el servicio creado (o null)</returns>
        public async Task<(bool Success, string Message, Servicio? Servicio)> AddServicioAsync(ServicioDto dto)
        {
            if (dto == null)
                return (false, "El servicio no puede ser nulo.", null);

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre del servicio es obligatorio.", null);

            var negocio = await _negocioRepo.GetByIdAsync(dto.NegocioId);
            if (negocio == null)
                return (false, $"No existe negocio con Id {dto.NegocioId}.", null);

            var servicio = dto.ToServicio();

            try
            {
                if (dto.Imagen != null && dto.Imagen.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "servicios");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{dto.Imagen.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Imagen.CopyToAsync(stream);
                    }

                    servicio.ImagenUrl = Path.Combine("uploads", "servicios", uniqueFileName).Replace("\\", "/");
                }

                await _servicioRepo.AddAsync(servicio);
                await _servicioRepo.SaveChangesAsync();

                return (true, "Servicio añadido correctamente.", servicio);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error al añadir servicio a la base de datos.");
                return (false, "Error al añadir servicio a la base de datos.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al añadir servicio.");
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string Message, byte[]? ImageBytes, string ContentType)> GetImagenByServicioIdAsync(int servicioId)
        {
            var servicio = await _servicioRepo.GetByIdAsync(servicioId);
            if (servicio == null)
                return (false, "Servicio no encontrado.", null, "");

            if (string.IsNullOrEmpty(servicio.ImagenUrl))
                return (false, "El servicio no tiene imagen asignada.", null, "");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), servicio.ImagenUrl.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(filePath))
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
        /// <summary>
        /// Actualiza los datos de un servicio existente.
        /// </summary>
        /// <param name="id">ID del servicio a actualizar</param>
        /// <param name="servicioDto">Datos nuevos del servicio</param>
        /// <returns>Tupla con éxito y mensaje</returns>
        public async Task<(bool Success, string Message, ServicioDto? ServicioActualizado)> UpdateServicioAsync(int id, ServicioUpdateDto servicioDto)
        {
            if (servicioDto == null)
                return (false, "El servicio no puede ser nulo.", null);

            // Buscar el servicio existente
            var servicioExistente = await _servicioRepo.GetByIdAsync(id);
            if (servicioExistente == null)
                return (false, "Servicio no encontrado.", null);

            // Actualizar manualmente cada propiedad con los datos del DTO
            servicioExistente.Nombre = servicioDto.Nombre;
            servicioExistente.Descripcion = servicioDto.Descripcion;
            servicioExistente.DuracionMinutos = servicioDto.DuracionMinutos;
            servicioExistente.Precio = servicioDto.Precio;

            try
            {
                // Actualizamos la entidad y guardamos cambios
                _servicioRepo.Update(servicioExistente);
                await _servicioRepo.SaveChangesAsync();

                // Convertir la entidad actualizada a DTO para devolverla
                var servicioActualizadoDto = new ServicioDto
                {
                    Nombre = servicioExistente.Nombre,
                    Descripcion = servicioExistente.Descripcion,
                    DuracionMinutos = servicioExistente.DuracionMinutos,
                    Precio = servicioExistente.Precio
                };

                return (true, "Servicio actualizado correctamente.", servicioActualizadoDto);
            }
            catch (DbUpdateException dbEx)
            {
                var detailedMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                _logger.LogError(dbEx, "Error al actualizar el servicio con ID {Id}. Detalles: {Detalle}", id, detailedMessage);
                return (false, $"Error al actualizar el servicio en la base de datos: {detailedMessage}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar servicio.");
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateImagenServicioAsync(int servicioId, Imagen nuevaImagen)
        {
            var imagen= nuevaImagen.Url;
            if (imagen == null || imagen.Length == 0)
                return (false, "La nueva imagen es inválida.");

            var servicio = await _servicioRepo.GetByIdAsync(servicioId);
            if (servicio == null)
                return (false, "Servicio no encontrado.");

            try
            {
                // Eliminar la imagen anterior si existe
                if (!string.IsNullOrWhiteSpace(servicio.ImagenUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), servicio.ImagenUrl.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);
                }

                // Guardar la nueva imagen
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "servicios");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{imagen.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                servicio.ImagenUrl = Path.Combine("uploads", "servicios", uniqueFileName).Replace("\\", "/");

                _servicioRepo.Update(servicio);
                await _servicioRepo.SaveChangesAsync();

                return (true, "Imagen del servicio actualizada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la imagen del servicio.");
                return (false, $"Error inesperado: {ex.Message}");
            }
        }
        /// <summary>
        /// Obtiene detalles de los servicios de un negocio, incluyendo datos relacionados como valoraciones y reservas.
        /// </summary>
        /// <param name="negocioId">ID del negocio</param>
        /// <returns>Tupla con éxito, mensaje y lista de detalles de servicios</returns>
        public async Task<(bool Success, string Message, List<ServicioDetalleDto> Servicios)> GetServiciosDetalleByNegocioIdAsync(int negocioId)
        {
            try
            {
                // Obtener negocio con categoría y valoraciones
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => n.Id == negocioId,
                    n => n.Categoria,
                    n => n.Valoraciones // Antes ResenasNegocio, ahora Valoraciones
                );

                var negocio = negocios.FirstOrDefault();

                if (negocio == null)
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<ServicioDetalleDto>());

                // Calcular promedio y total de valoraciones del negocio
                var valoraciones = negocio.Valoraciones.ToList();
                double promedio = valoraciones.Any() ? valoraciones.Average(v => v.Puntuacion) : 0.0;
                int totalValoraciones = valoraciones.Count;

                // Obtener servicios del negocio
                var servicios = await _servicioRepo.GetWhereAsync(
                    s => s.NegocioId == negocioId,
                    query => query
                        .Include(s => s.Negocio)
                            .ThenInclude(n => n.Categoria)
                        .AsNoTracking()
                );

                if (servicios == null || servicios.Count == 0)
                    return (true, $"No se encontraron servicios para el negocio con ID {negocioId}.", new List<ServicioDetalleDto>());

                // Obtener todas las reservas del negocio (para contar reservas por servicio)
                var reservas = await _reservaRepo.GetWhereAsync(r => r.NegocioId == negocioId);

                // Proyectar a DTO usando el método estático FromServicio
                var detalles = servicios.Select(s =>
                {
                    int numeroReservas = reservas.Count(r => r.ServicioId == s.Id);
                    return ServicioDetalleDto.FromServicio(s, negocio, promedio, totalValoraciones, numeroReservas);
                }).ToList();

                return (true, "Servicios detallados obtenidos correctamente.", detalles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener servicios detallados del negocio con ID {negocioId}");
                return (false, $"Error al obtener servicios detallados: {ex.Message}", new List<ServicioDetalleDto>());
            }
        }



        /// <summary>
        /// Elimina un servicio dado su ID.
        /// </summary>
        /// <param name="id">ID del servicio</param>
        /// <returns>Tupla con éxito y mensaje</returns>
        public async Task<(bool Success, string Message)> DeleteServicioAsync(int id)
        {
            try
            {
                // Busca el servicio a eliminar
                var servicio = await _servicioRepo.GetByIdAsync(id);
                if (servicio == null)
                    return (false, "Servicio no encontrado.");

                // Elimina el servicio y guarda cambios
                await _servicioRepo.DeleteAsync(servicio);
                await _servicioRepo.SaveChangesAsync();
                return (true, "Servicio eliminado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio.");
                return (false, $"Error al eliminar servicio: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, List<ServicioDetalleDto> Servicios)> GetServiciosDetalleAsync()
        {
            try
            {
                // Obtener negocios activos y no bloqueados con sus categorías y valoraciones
                var negocios = await _negocioRepo.GetWhereWithIncludesAsync(
                    n => n.Activo && !n.Bloqueado,
                    n => n.Categoria,
                    n => n.Valoraciones
                );

                var negocioIds = negocios.Select(n => n.Id).ToList();

                // Obtener servicios cuyos negocios están activos y no bloqueados, incluyendo negocio y categoría
                var servicios = await _servicioRepo.GetWhereAsync(
                    s => negocioIds.Contains(s.NegocioId),
                    query => query
                        .Include(s => s.Negocio)
                            .ThenInclude(n => n.Categoria)
                );

                var servicioIds = servicios.Select(s => s.Id).ToList();

                // Obtener reservas solo para estos servicios
                var reservas = await _reservaRepo.GetWhereAsync(r => servicioIds.Contains(r.ServicioId));

                // Mapear valoraciones por negocio (promedio y total)
                var negocioValoracionesMap = negocios.ToDictionary(
                    n => n.Id,
                    n => new
                    {
                        Promedio = n.Valoraciones.Any() ? n.Valoraciones.Average(v => v.Puntuacion) : 0.0,
                        Total = n.Valoraciones.Count
                    }
                );

                // Mapear detalles de servicios con sus negocios, valoraciones y número de reservas
                var detalles = servicios.Select(s =>
                {
                    var negocioId = s.NegocioId;
                    var valoracion = negocioValoracionesMap.ContainsKey(negocioId)
                        ? negocioValoracionesMap[negocioId]
                        : new { Promedio = 0.0, Total = 0 };

                    return ServicioDetalleDto.FromServicio(
                        servicio: s,
                        negocio: s.Negocio,
                        valoracionPromedioNegocio: valoracion.Promedio,
                        numeroValoracionesNegocio: valoracion.Total,
                        numeroReservas: reservas.Count(r => r.ServicioId == s.Id)
                    );
                }).ToList();

                return (true, "Servicios detallados obtenidos correctamente.", detalles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios detallados");
                return (false, $"Error al obtener servicios detallados: {ex.Message}", new List<ServicioDetalleDto>());
            }
        }
    }
}
