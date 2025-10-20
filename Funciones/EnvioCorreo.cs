using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace ProyDesaWeb2025.Funciones
{
    public class EnvioCorreo
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EnvioCorreo()
        {
            // Las variables se configuran en Render (Environment Variables)
            _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "";
            _fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL") ?? "rdra20.ies@gmail.com";
            _fromName = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME") ?? "Sistema TeckeyGT";
        }

        public async Task EnviarCorreoConPDF(string correoDestino, string nombreUsuario, byte[] pdf)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(correoDestino, nombreUsuario);
                var subject = "Carnet de Registro - TeckeyGT";

                var plainTextContent = $"Hola {nombreUsuario}, gracias por registrarte. En el archivo adjunto encontrarás tu carnet digital.";
                var htmlContent = $"<p>Hola <strong>{nombreUsuario}</strong>, gracias por registrarte.</p>" +
                                  $"<p>En el archivo adjunto encontrarás tu carnet digital.</p>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                // Adjuntar el PDF
                msg.AddAttachment("CarnetUsuario.pdf", Convert.ToBase64String(pdf), "application/pdf");

                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"📧 SendGrid status: {response.StatusCode}");

                if ((int)response.StatusCode >= 400)
                {
                    var body = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"⚠️ Error SendGrid: {body}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo: {ex.Message}");
                throw;
            }
        }
    }
}
