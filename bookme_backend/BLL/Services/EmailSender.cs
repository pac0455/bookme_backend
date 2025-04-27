    using bookme_backend.DataAcces.Models;
    using Microsoft.AspNetCore.Identity;

    namespace bookme_backend.BLL.Services
    {
        using MailKit.Net.Smtp;
        using MimeKit;
        using Microsoft.Extensions.Configuration;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Identity.UI.Services;
        using MailKit.Security;
        using Microsoft.AspNetCore.Identity;

        public class EmailSender : IEmailSender<Usuario>
        {
            private readonly IConfiguration _configuration;
            private readonly ILogger<EmailSender> _logger;

            public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
            {
                _configuration = configuration;
                _logger = logger;
            }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"Intento de enviar email a {email}");

            // Implementación SIMPLIFICADA para pruebas
            try
            {
                // Solo loguear en desarrollo
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    _logger.LogInformation($"Email simulado para {email}: {subject}\n{htmlMessage}");
                    return;
                }

                // Implementación real SMTP aquí...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error REAL enviando email");
                throw;
            }
        }


        public Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink)
                => SendEmailAsync(email, "Confirmación de cuenta",
                    $"<p>Hola {user.UserName},</p><p>Por favor confirma tu cuenta haciendo <a href='{confirmationLink}'>clic aquí</a>.</p>");

            public Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink)
                => SendEmailAsync(email, "Restablecer contraseña",
                    $"<p>Hola {user.UserName},</p><p>Para restablecer tu contraseña, haz <a href='{resetLink}'>clic aquí</a>.</p>");

            public Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode)
                => SendEmailAsync(email, "Código de restablecimiento",
                    $"<p>Hola {user.UserName},</p><p>Tu código para restablecer la contraseña es: <strong>{resetCode}</strong></p>");
        }

    }
