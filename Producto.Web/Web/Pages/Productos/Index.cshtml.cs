
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Text.Json;
using Abstracciones.Reglas;
using Abstracciones.Modelos;

namespace Web.Pages.Productos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IConfiguracion _configuracion;
        public IList<ProductoResponse> productos { get; set; } = default!;
        
        public IndexModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGet()
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("APIEndPoints", "ObtenerProductos");
                using var cliente = ObtenerClienteConToken();
                var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var respuesta = await cliente.SendAsync(solicitud);
                respuesta.EnsureSuccessStatusCode();

                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                productos = JsonSerializer.Deserialize<List<ProductoResponse>>(resultado, opciones) ?? new List<ProductoResponse>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener productos: {ex.Message}");
                productos = new List<ProductoResponse>();
            }
        }
        
        // ★ Helper — extrae el JWT de los claims y configura el HttpClient
        private HttpClient ObtenerClienteConToken()
        {
            var tokenClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "AccessToken");
            var cliente = new HttpClient();
            if (tokenClaim != null)
                cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", tokenClaim.Value);
            return cliente;
        }
    }
}
