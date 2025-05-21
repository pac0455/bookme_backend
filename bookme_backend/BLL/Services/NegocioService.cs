using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.BLL.Services
{
    public class NegocioService(
        IRepository<Horario> horarioRepo,
        IRepository<Negocio> negocioRepo,
        IRepository<Suscripcion> subcripcionesRepo,
        ILogger<NegocioService> logger
        ) : INegocioService
    {

        private readonly IRepository<Negocio> _negocioRepo = negocioRepo;
        private readonly IRepository<Suscripcion> _subcripcionesRepo = subcripcionesRepo;
        private readonly IRepository<Horario> _horarioRepo = horarioRepo;
        private readonly ILogger<NegocioService> _logger=logger;



        //Crea un negocio y lo relaciona con el usuario mediante una subcripción
        public async Task<(bool Success, string Message)> AddAsync(Negocio negocio, string usuarioId)
        {
            try
            {
                await _negocioRepo.AddAsync(negocio);
                await _negocioRepo.SaveChangesAsync();

                _logger.LogInformation($"Total horarios recibidos: {negocio.HorariosAtencion?.Count()}");

                //foreach (var horario in negocio.HorariosAtencion)
                //{
                //    _logger.LogInformation($"Insertando horario: {horario.DiaSemana} - {horario.HoraInicio} - {horario.HoraFin}");
                //    horario.Id = 0;
                //    horario.IdNegocio = negocio.Id;
                //    await _horarioRepo.AddAsync(horario);
                //}


                //await _horarioRepo.SaveChangesAsync();

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
                    n => n.HorariosAtencion  // Incluir horarios
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

            if (!string.Equals(negocioActual.Categoria, negocio.Categoria, StringComparison.Ordinal))
                negocioActual.Categoria = negocio.Categoria;

            _negocioRepo.Update(negocioActual);
            await _negocioRepo.SaveChangesAsync();

            return (true, "Negocio actualizado correctamente.");
        }
    }
}
