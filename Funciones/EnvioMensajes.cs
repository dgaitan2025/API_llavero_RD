using Api_Empleados.Funciones;
using System.Collections.Generic;

namespace ProyDesaWeb2025.Funciones
{
    public class EnvioMensajes
    {
        private readonly EnvioCorreo _emailService;
        private readonly TwilioMsg _twilio;
        private readonly CarnetGenerador _carnetGen;

        private readonly IConfiguration _cfg;
        private readonly string _baseUrl;
        public EnvioMensajes(
    EnvioCorreo emailService,
    TwilioMsg twilio,
    CarnetGenerador carnetGen,
    IConfiguration cfg)   // ⬅️ inyectado desde Program.cs
        {
            _emailService = emailService;
            _twilio = twilio;
            _carnetGen = carnetGen;
            _cfg = cfg;
            _baseUrl = _cfg["AppBaseUrl"] ?? "";
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
            var rutaPlantilla = Path.Combine("Recursos", "IMGS", "Plantilla.png");
            var pdf = _carnetGen.GenerarCarnetConPlantilla(
                nombre,
                nickname,
                rutaPlantilla
            );

            // Guardar PDF en servidor
            var rutaPdf = Path.Combine("Recursos", "PDFS");
            Directory.CreateDirectory(rutaPdf);

            string fileName = $"{nickname}.pdf";
            string fullPath = Path.Combine(rutaPdf, fileName);
            await File.WriteAllBytesAsync(fullPath, pdf);

            /* ─────────── Enviar correo ─────────── */
            await _emailService.EnviarCorreoConPDF(correo, nombre, pdf);
            string pdfUrl = $"{_baseUrl}/Recursos/PDFS/{fileName}";

            /* ─────────── Enviar WhatsApp ─────────── */
            var vars = new Dictionary<string, string>
            {
                ["1"] = nickname,   // {{1}}
                ["2"] = pdfUrl// solo nombre, sin .pdf
            };

            // Construir la URL pública

            // Enviar WhatsApp
            _twilio.SendWhatsAppTemplate(
                toE164: $"+502{telefono}",
                contentSid: contentSid,
                vars: vars,
                pdfPath: fullPath
            );
        }
    }
}
