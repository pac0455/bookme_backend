using System.Threading.Tasks;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.BLL.Services
{
    /// <summary>
    /// Servicio para el envío de emails de la aplicación
    /// </summary>
    public interface ICustomEmailSender
    {
        /// <summary>
        /// Envía un email genérico
        /// </summary>
        /// <param name="email">Dirección de email del destinatario</param>
        /// <param name="subject">Asunto del email</param>
        /// <param name="htmlMessage">Contenido del email en HTML</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task SendEmailAsync(string email, string subject, string htmlMessage);

        /// <summary>
        /// Envía el enlace de confirmación de cuenta
        /// </summary>
        /// <param name="user">Usuario destinatario</param>
        /// <param name="email">Email del destinatario</param>
        /// <param name="confirmationLink">Enlace de confirmación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink);

        /// <summary>
        /// Envía el enlace para restablecer contraseña
        /// </summary>
        /// <param name="user">Usuario destinatario</param>
        /// <param name="email">Email del destinatario</param>
        /// <param name="resetLink">Enlace de restablecimiento</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink);

        /// <summary>
        /// Envía el código para restablecer contraseña
        /// </summary>
        /// <param name="user">Usuario destinatario</param>
        /// <param name="email">Email del destinatario</param>
        /// <param name="resetCode">Código de restablecimiento</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode);
    }
}