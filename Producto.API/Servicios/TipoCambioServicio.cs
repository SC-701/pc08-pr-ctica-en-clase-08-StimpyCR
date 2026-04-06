using Abstracciones.Interfaces.Reglas;
using Abstracciones.Interfaces.Servicios;
using Abstracciones.Modelos.Servicios.CambioDolar;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Servicios
{
    public class TipoCambioServicio : ITipoCambioDolar
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguracion _config;

        public TipoCambioServicio(HttpClient httpClient, IConfiguracion config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<CambioDolar> ObtenerTipoCambioDolar(string fecha)
        {
            var baseUrl = _config.ObtenerMetodo("BancoCentralCR", "UrlBase");
            var token = _config.ObtenerMetodo("BancoCentralCR", "BearerToken");
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("BancoCentralCR:UrlBase no está configurado.");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("BancoCentralCR:BearerToken no está configurado.");

            if (!DateTime.TryParse(fecha, out var fechaConsulta))
                fechaConsulta = DateTime.UtcNow;

            var fechaParam = fechaConsulta.ToString("yyyy/MM/dd");
            var url = $"{baseUrl}?fechaInicio={fechaParam}&fechaFin={fechaParam}&idioma=ES";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException("BCCR 401 Unauthorized: revisa el BearerToken.");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("estado", out var estado) || !estado.GetBoolean())
                throw new InvalidOperationException("Respuesta del BCCR inválida o sin datos.");
            if (!root.TryGetProperty("datos", out var datosElem) || datosElem.ValueKind != JsonValueKind.Array || datosElem.GetArrayLength() == 0)
                throw new InvalidOperationException("La respuesta del BCCR no contiene 'datos'.");

            // Busca indicador de venta (código 318)
            JsonElement? indicadorVenta = null;
            var datos0 = datosElem[0];
            if (datos0.TryGetProperty("indicadores", out var indicadoresElem) && indicadoresElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var ind in indicadoresElem.EnumerateArray())
                {
                    if (ind.TryGetProperty("codigoIndicador", out var codigoElem) && codigoElem.GetString() == "318")
                    {
                        indicadorVenta = ind;
                        break;
                    }
                }
            }

            if (indicadorVenta is null)
                throw new InvalidOperationException("No se encontró el indicador de venta (código 318) en la respuesta del BCCR.");

            var indVenta = indicadorVenta.Value;
            if (!indVenta.TryGetProperty("series", out var seriesElem) || seriesElem.ValueKind != JsonValueKind.Array || seriesElem.GetArrayLength() == 0)
                throw new InvalidOperationException("El indicador de venta no contiene 'series'.");

            // Toma la primera serie disponible para la fecha consultada
            var serie = seriesElem[0];
            var fechaSerie = serie.TryGetProperty("fecha", out var fElem) ? (fElem.GetString() ?? fechaConsulta.ToString("yyyy-MM-dd")) : fechaConsulta.ToString("yyyy-MM-dd");
            var valor = serie.TryGetProperty("valorDatoPorPeriodo", out var vElem) ? vElem.GetDouble() : 0d;

            if (valor <= 0)
                throw new InvalidOperationException("El BCCR devolvió un tipo de cambio inválido (<= 0).");

            return new CambioDolar
            {
                fecha = fechaSerie,
                valorDatoPorPeriodo = valor
            };
        }
    }
}
