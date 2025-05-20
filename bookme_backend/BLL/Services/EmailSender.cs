using MailKit.Net.Smtp;
using MimeKit;


using bookme_backend.DataAcces.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace bookme_backend.BLL.Services
{
    public class EmailSender : ICustomEmailSender
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
            try
            {


                _logger.LogInformation($"Simulación de email para {email}: {subject}\n{htmlMessage}");
                // Configuración del mensaje
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:SenderName"],
                    _configuration["EmailSettings:SenderEmail"]));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                // Cuerpo del mensaje en HTML
                var builder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };

                message.Body = builder.ToMessageBody();

                // Configuración del cliente SMTP
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["EmailSettings:MailServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"]),
                    SecureSocketOptions.StartTlsWhenAvailable);

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:SenderPassword"]);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado correctamente a {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email");
                throw; // Re-lanzar la excepción para que el llamador sepa que falló
            }
        }

        public Task SendConfirmationLinkAsync(Usuario user, string email, string confirmationLink)
            => SendEmailAsync(email, "Confirmación de cuenta",
                $@"<h1>Confirma tu cuenta</h1>
                   <p>Hola {user.UserName},</p>
                   <p>Por favor confirma tu cuenta haciendo <a href='{confirmationLink}'>clic aquí</a>.</p>
                   <p>Si no solicitaste este registro, ignora este mensaje.</p>");

        public Task SendPasswordResetLinkAsync(Usuario user, string email, string resetLink)
            => SendEmailAsync(email, "Restablecer contraseña",
                $@"<h1>Restablecer contraseña</h1>
                   <p>Hola {user.UserName},</p>
                   <p>Para restablecer tu contraseña, haz <a href='{resetLink}'>clic aquí</a>.</p>
                   <p>Este enlace expirará en 24 horas.</p>");

        public Task SendPasswordResetCodeAsync(Usuario user, string email, string resetCode)
            => SendEmailAsync(email, "Código de restablecimiento",
                $@"<h1>Código de verificación</h1>
                   <p>Hola {user.UserName},</p>
                   <p>Tu código para restablecer la contraseña es:</p>
                   <h2>{resetCode}</h2>
                   <p>Este código expirará en 10 minutos.</p>");
    }
}