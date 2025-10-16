using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ProyDesaWeb2025.Funciones
{
    public class FaceApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public FaceApiClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _baseUrl = cfg["FaceApi:BaseUrl"] ?? "https://www.server.daossystem.pro:3405";
        }

        /// <summary>
        /// Compara dos rostros contra la API externa.
        /// </summary>
        /// <param name="fotoA">Bytes de la primera foto (ej. de la BD)</param>
        /// <param name="fotoB">Bytes de la segunda foto (ej. subida por el cliente)</param>
        /// <returns>true si hay coincidencia, false si no</returns>
        public async Task<bool> CompareAsync(byte[] fotoA,string fotoB)
        {
            // Convertir a base64
            string base64A = Convert.ToBase64String(fotoA);
            

            var payload = new
            {
                RostroA = base64A,
                RostroB = fotoB
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync($"{_baseUrl}/Rostro/Verificar", content);
            if (!response.IsSuccessStatusCode)
                return false;

            // Dependiendo de la API, ajusta aquí el parseo de la respuesta
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("coincide", out var coincide))
                    return coincide.GetBoolean();

                if (doc.RootElement.TryGetProperty("resultado", out var resultado))
                    return resultado.GetBoolean();
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
