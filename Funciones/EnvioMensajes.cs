using Api_Empleados.Funciones;
using System.Collections.Generic;

namespace ProyDesaWeb2025.Funciones
{
    public class EnvioMensajes
    {
        private readonly EnvioCorreo _emailService;
        private readonly TwilioMsg _twilio;
        private readonly CarnetGenerador _carnetGen;
        private readonly IConfiguration cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        private readonly string _baseUrl;

        public EnvioMensajes(
            EnvioCorreo emailService,
            TwilioMsg twilio,
            CarnetGenerador carnetGen)
        {
            _emailService = emailService;
            _twilio = twilio;
            _carnetGen = carnetGen;
            _baseUrl = cfg["AppBaseUrl"] ?? "";
        }

        /// <summary>
        /// Genera el carnet PDF y lo envía por correo y WhatsApp.
        /// </summary>
        public async Task EnviarTodoAsync(
            string correo,
            string nombre,
            string nickname,
            string telefono,
            string contentSid)
        {
            /* ─────────── Generar PDF ─────────── */
            var rutaPlantilla = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "recursos", "imgs", "Plantilla.png"
            );

            var pdf = _carnetGen.GenerarCarnetConPlantilla(
                nombre,
                nickname,
                rutaPlantilla
            );

            /* ─────────── Guardar PDF en servidor ─────────── */
            var rutaPdf = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "recursos", "pdfs"
            );
            Directory.CreateDirectory(rutaPdf);

            string fileName = $"{nickname}.pdf";
            string fullPath = Path.Combine(rutaPdf, fileName);

            await File.WriteAllBytesAsync(fullPath, pdf);

            // URL pública del PDF (para correo o WhatsApp)
            string pdfUrl = $"{_baseUrl}/recursos/pdfs/{fileName}";

            /* ─────────── Enviar correo ─────────── */
            await _emailService.EnviarCorreoConPDF(correo, nombre, pdf);

            /* ─────────── Enviar WhatsApp ─────────── */
            var vars = new Dictionary<string, string>
            {
                ["1"] = nickname,
                ["2"] = pdfUrl
            };

            _twilio.SendWhatsAppTemplate(
                toE164: $"+502{telefono}",
                contentSid: contentSid,
                vars: vars,
                pdfPath: fullPath
            );
        }
    }
}
