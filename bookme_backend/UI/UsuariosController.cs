using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using FirebaseAdmin.Auth;
using bookme_backend.BLL.Interfaces;
using bookme_backend.BLL.Services;
using bookme_backend.DataAcces.DTO;
using bookme_backend.BLL.Exceptions;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService usuarioService;

        // Inyección de dependencias para el repositorio genérico
        public UsuariosController(IUsuarioService usuarioService)
        {
            this.usuarioService = usuarioService;
        }
        [HttpPost("signup/google")]
        public async Task<IActionResult> SignupWithGoogle([FromBody] UsuarioRegistroDto dto)
        {
            try
            {
                var usuario = new Usuario
                {
                    FirebaseUid = dto.FirebaseUid,
                    Email = dto.Email,
                    UserName = dto.UserName,
                    PhoneNumber = dto.PhoneNumber,
                    PasswordHash = dto.Password
                };
                await usuarioService.CrearUsuarioAsync(usuario);
                // Aquí puedes mapear reservas si es necesario

                return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
            }
            catch (EntityDuplicatedException)
            {
                return BadRequest(new { mensaje = "Ya existe un usuario con ese email." });
            }
            catch (Exception ex) { return BadRequest(new { mensaje = $"{ex.Message} + \n: InnerException: {ex.InnerException}" }); }

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(string id)
        {
            var usuario = await usuarioService.GetByEmailAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
        {
            try
            {
                var usuarios = await usuarioService.GetAllAsync();
                return Ok(usuarios);
            }
            catch(Exception ex)
            {
                return BadRequest(new {description = ex});
            }
           
        }
        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            var creado = await usuarioService.CrearUsuarioAsync(usuario);

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            // Usar eliminación lógica (soft delete) si es necesario
            var reponse= await usuarioService.DeleteAsync(id);
            return NoContent();
        }
    }
}
