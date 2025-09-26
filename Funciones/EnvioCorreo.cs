using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace ProyDesaWeb2025.Funciones
{
    public class EnvioCorreo
    {
        public async Task EnviarCorreoConPDF(string correoDestino, string nombreUsuario, byte[] pdf)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema", "rdra20.ies@gmail.com")); // Cambiar
            message.To.Add(new MailboxAddress(nombreUsuario, correoDestino));
            message.Subject = "Carnet de Registro";

            var builder = new BodyBuilder
            {
                TextBody = $"Hola {nombreUsuario}, gracias por registrarte. En el archivo adjunto encontrarás tu carnet digital.",
            };

            builder.Attachments.Add("CarnetUsuario.pdf", pdf, new ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("rdra20.ies@gmail.com", "sjammhmtreziblfp");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
