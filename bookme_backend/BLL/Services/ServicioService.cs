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

                // Obtenemos las reservas del negocio, incluyendo los servicios reservados
                var reservas = await _reservaRepo.GetWhereWithIncludesAsync(
                    r => r.NegocioId == negocioId,
                    r => r.ReservasServicios
                );




                // Extraemos los servicios de todas las reservas, eliminamos duplicados y filtramos nulos
                var serviciosReservados = reservas
                    .SelectMany(r => r.ReservasServicios)    // Aplanamos la colección de ReservasServicios para obtener todos los servicios reservados
                    .Select(rs => rs.Servicio)               // Seleccionamos el servicio de cada ReservasServicio
                    .Where(s => s != null)                   // Filtramos servicios nulos (por seguridad)
                    .DistinctBy(s => s.Id)                   // Eliminamos duplicados basados en el Id del servicio
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
            return await _servicioRepo.GetAllAsync();
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

            var servicio = new Servicio
            {
                NegocioId = dto.NegocioId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                DuracionMinutos = dto.DuracionMinutos,
                Precio = dto.Precio
            };

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
                    DuracionMinutos = servicioExistente.DuracionMinutos ?? -1,
                    Precio = servicioExistente.Precio ?? -1
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
                // Validar si el negocio existe
                var negocioExiste = await _negocioRepo.Exist(n => n.Id == negocioId);
                if (!negocioExiste)
                    return (false, $"El negocio con ID {negocioId} no existe.", new List<ServicioDetalleDto>());

                // Cargar servicios filtrando por negocio, e incluir entidades relacionadas con Include para evitar múltiples consultas
                var servicios = await _servicioRepo.GetWhereAsync(
                     s => s.NegocioId == negocioId,
                     query => query.AsNoTracking()
                 );

                var detalles = servicios.Select(s =>
                {
                    var reservas = s.ReservasServicios
                        .Select(rs => rs.Reserva)
                        .Where(r => r != null)
                        .ToList();

                    var valoraciones = reservas
                        .SelectMany(r => r.Valoraciones)
                        .ToList();
                    return new ServicioDetalleDto
                    {
                        Id = s.Id,
                        NegocioId = s.NegocioId,
                        Nombre = s.Nombre ?? string.Empty,
                        Descripcion = s.Descripcion ?? string.Empty,
                        DuracionMinutos = s.DuracionMinutos ?? 0,
                        Precio = s.Precio ?? 0,
                        NegocioNombre = s.Negocio?.Nombre ?? string.Empty,
                        Categoria = s.Negocio?.Categoria ?? "Sin categoría",
                        ValoracionPromedio = valoraciones.Any() ? valoraciones.Average(v => v.Puntuacion ?? 0) : 0,
                        NumeroValoraciones = valoraciones.Count,
                        NumeroReservas = reservas.Count
                    };

                }).ToList();



                // Validar que existan servicios para el negocio
                if (servicios == null || servicios.Count == 0)
                    return (true, $"No se encontraron servicios para el negocio con ID {negocioId}.", new List<ServicioDetalleDto>());

                // Proyectamos los servicios a DTO con cálculos de valoraciones y reservas
                var serviciosDetalles = servicios.Select(s =>
                {
                    // Extraemos las reservas que tiene el servicio, filtrando nulos por seguridad
                    var reservas = s.ReservasServicios.Select(rs => rs.Reserva).Where(r => r != null).ToList();

                    // De todas las reservas, extraemos las valoraciones asociadas
                    var valoraciones = reservas.SelectMany(r => r.Valoraciones).ToList();

                    // Calculamos la valoración promedio, si hay valoraciones; si no, asignamos 0.0
                    double valoracionPromedio = valoraciones.Any()
                        ? valoraciones.Average(v => v.Puntuacion ?? 0)
                        : 0.0;

                    // Número total de valoraciones
                    int numeroValoraciones = valoraciones.Count;

                    // Número total de reservas del servicio
                    int numeroReservas = reservas.Count;

                    return new ServicioDetalleDto
                    {
                        Id = s.Id,
                        NegocioId = s.NegocioId,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        DuracionMinutos = s.DuracionMinutos,
                        Precio = s.Precio,
                        NegocioNombre = s.Negocio?.Nombre ?? "",
                        Categoria = "Sin categoría",
                        ValoracionPromedio = valoracionPromedio,
                        NumeroValoraciones = numeroValoraciones,
                        NumeroReservas = numeroReservas
                    };
                }).ToList();

                return (true, "Servicios detallados obtenidos correctamente.", serviciosDetalles);
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
    }
}
