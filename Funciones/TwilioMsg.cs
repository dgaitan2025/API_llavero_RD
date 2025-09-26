using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using System.Text.Json;

namespace ProyDesaWeb2025.Funciones
{
    public class TwilioMsg
    {
        private readonly string _smsFrom;
        private readonly string _waFrom;
        private readonly string _baseUrl;
        private readonly string _pdfFolder;

        public TwilioMsg(IConfiguration cfg, IHostEnvironment env)
        {
            TwilioClient.Init(cfg["Twilio:AccountSID"], cfg["Twilio:AuthToken"]);

            _smsFrom = cfg["Twilio:FromPhoneNumber"];
            _waFrom = cfg["Twilio:FromWhatsAppNumber"] ?? "whatsapp:+14155238886";
            _pdfFolder = Path.Combine(env.ContentRootPath, "Recursos", "PDFS");
            Directory.CreateDirectory(_pdfFolder);
        }

        private static string Wa(string e164) =>
            e164.StartsWith("+") ? $"whatsapp:{e164}" : $"whatsapp:+{e164}";

        public MessageResource SendWhatsAppTemplate(
            string toE164,
            string contentSid,
            IDictionary<string, string> vars,
            string? pdfPath = null)
        {

            var opts = new CreateMessageOptions(new PhoneNumber(Wa(toE164)))
            {
                From = new PhoneNumber(_waFrom),
                ContentSid = contentSid,
                ContentVariables = JsonSerializer.Serialize(vars)
            };

            return MessageResource.Create(opts);
        }
    }

}
