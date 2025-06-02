using bookme_backend.BLL.Interfaces;
using bookme_backend.DataAcces.DTO.Valoraciones;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace bookme_backend.BLL.Services
{
    public class ValoracionesService : IValoracionesService
    {
        private readonly IRepository<Valoracion> _valoracionRepo;
        private readonly IRepository<Negocio> _negocioRepo;
        private readonly UserManager<Usuario> _userManager;



        public ValoracionesService(
            IRepository<Valoracion> valoracionRepo,
            UserManager<Usuario> userManager,
            IRepository<Negocio> negocioRepo
)
        {
            _valoracionRepo = valoracionRepo;
            _negocioRepo = negocioRepo;
            _userManager = userManager;
        }

        public async Task<(bool Succes, string Message, List<ValoracionResponseDTO> valoraciones)> GetValoracionesByNegocioId(int negocioID)
        {
            try
            {
                var existNegocio = await _negocioRepo.Exist(x => x.Id == negocioID);
                if (!existNegocio)
                {
                    return (false, "El negocio no existe", []);
                }

                var valoraciones = await _valoracionRepo.GetWhereWithIncludesAsync(
                    v => v.NegocioId == negocioID,
                    v => v.Usuario
                );

                var dtoList = valoraciones.Select(v => new ValoracionResponseDTO
                {
                    Id = v.Id,
                    NegocioId = v.NegocioId,
                    UsuarioId = v.UsuarioId,
                    Puntuacion = v.Puntuacion,
                    Comentario = v.Comentario,
                    FechaValoracion = v.FechaValoracion,
                    Usuario = new UsuarioDTO
                    {
                        Id = v.Usuario.Id,
                        UserName = v.Usuario.UserName,
                        Email = v.Usuario.Email
                    }
                }).ToList();

                return (true, "Valoraciones cargadas correctamente", dtoList);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener valoraciones: {ex.Message}", new List<ValoracionResponseDTO>());
            }
        }

        public async Task<(bool Succes, string Message, ValoracionResponseDTO? valoracionCreada)> Create(ValoracionCreateDTO dto)
        {
            try
            {

                var existNegocio = await _negocioRepo.Exist(x => x.Id == dto.NegocioId);
                if (!existNegocio)
                {
                    return (false, "El negocio no existe", null);
                } 
                var existeUsuario = await _userManager.FindByIdAsync(dto.UsuarioId);
                if (existeUsuario == null)
                {
                    return (false, "El usuario no existe", null);
                }

                var nueva = new Valoracion
                {
                    NegocioId = dto.NegocioId,
                    UsuarioId = dto.UsuarioId,
                    Puntuacion = dto.Puntuacion,
                    Comentario = dto.Comentario,
                    FechaValoracion = DateTime.UtcNow
                };

                await _valoracionRepo.AddAsync(nueva);
                await _valoracionRepo.SaveChangesAsync();

                // Cargar con relaciones
                var valoracionConUsuario = await _valoracionRepo.GetWhereWithIncludesAsync(
                    v => v.Id == nueva.Id,
                    v => v.Usuario
                );

                var v = valoracionConUsuario.First();

                var response = new ValoracionResponseDTO
                {
                    Id = v.Id,
                    NegocioId = v.NegocioId,
                    UsuarioId = v.UsuarioId,
                    Puntuacion = v.Puntuacion,
                    Comentario = v.Comentario,
                    FechaValoracion = v.FechaValoracion,
                    Usuario = new UsuarioDTO
                    {
                        Id = v.Usuario.Id,
                        UserName = v.Usuario.UserName,
                        Email = v.Usuario.Email
                    }
                };

                return (true, "Valoración creada correctamente", response);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la valoración: {ex.Message}", null!);
            }
        }
    }
}
