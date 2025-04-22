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

        // GET: api/Usuarios
   
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            try
            {
                var usuario = await usuarioService.GetByIdAsync(id);
                return Ok(usuario);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

  

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            // Aquí solo actualizas el usuario usando el repositorio genérico
            usuarioService.Update(usuario);

            try
            {
                await usuarioService.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        [HttpPost("auth/google")]
        public async Task<IActionResult> LoginConGoogle([FromBody] string idToken)
        {
            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;

                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

                // Verifica si ya existe el usuario
                var usuarioExistente = await usuarioService.ObtenerPorFirebaseUidAsync(uid);
                if (usuarioExistente != null)
                {
                    return Ok(usuarioExistente); // Ya existe
                }

                // Si no existe, lo creas
                var nuevoUsuario = new Usuario
                {
                    Nombre = firebaseUser.DisplayName,
                    Email = firebaseUser.Email,
                    Telefono = firebaseUser.PhoneNumber,
                    FirebaseUid = firebaseUser.Uid,
                    FechaRegistro = DateTime.UtcNow,
                    Rol = ERol.Cliente // O el que prefieras
                };

                var creado = await usuarioService.CrearUsuarioAsync(nuevoUsuario);
                return CreatedAtAction("GetUsuario", new { id = nuevoUsuario.Id }, nuevoUsuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al autenticar con Google", error = ex.Message });
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

        private bool UsuarioExists(int id)
        {
            return usuarioService.GetAllAsync().Result.Any(e => e.Id == id);
        }
    }
}
