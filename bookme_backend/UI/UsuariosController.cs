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
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace bookme_backend.UI
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService usuarioService;
        private readonly UserManager<Usuario> _userManager;



        // Inyección de dependencias para el repositorio genérico
        public UsuariosController(IUsuarioService usuarioService, UserManager<Usuario> userManager)
        {
            this.usuarioService = usuarioService;
            _userManager = userManager;

        }
        //login
        [HttpPost("login")]
        public async Task<ActionResult<Usuario>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var usuario = await usuarioService.Login(loginRequest.Email, loginRequest.Password);
                return Ok(usuario); // Aquí podrías devolver un DTO o JWT
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "El usuario no existe." });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = $"Error: {ex.Message}" });
            }
        }
       


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UsuarioRegistroDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new Usuario
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirebaseUid = model.FirebaseUid
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok("Usuario registrado exitosamente");
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

                if (usuarios == null) return NotFound("Objeto usuario no encontrado");
                
                return Ok(usuarios);
            }
            catch(Exception ex)
            {
                return BadRequest(new {description = ex.StackTrace});
            }
           
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
