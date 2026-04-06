using Abstracciones.Reglas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;
using static Abstracciones.Modelos.ProductoBase;

namespace Web.Pages.Productos
{
    [Authorize]
    public class AgregarModel : PageModel
    {
        private IConfiguracion _configuracion;
        [BindProperty]
        public ProductoRequest producto { get; set; } = default!;
        [BindProperty]
        public List<SelectListItem> Categoria { get; set; } = new List<SelectListItem>();
        [BindProperty]
        public List<SelectListItem> subCategoria { get; set; } = new List<SelectListItem>();
        public Guid categoriaSeleccionada { get; set; } = default!;

        public AgregarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGetAsync()
        {
            await CargarCategorias();
            System.Diagnostics.Debug.WriteLine($"Categorías cargadas: {Categoria.Count}");
            foreach (var cat in Categoria)
            {
                System.Diagnostics.Debug.WriteLine($"  - {cat.Value}: {cat.Text}");
            }
        }

        public async Task<ActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return Page();
            
            string endpoint = _configuracion.ObtenerMetodo("APIEndPoints", "CrearProducto");
            using var cliente = ObtenerClienteConToken();

            var respuesta = await cliente.PostAsJsonAsync(endpoint, producto);
            respuesta.EnsureSuccessStatusCode();
            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetObtenerSubCategorias(Guid categoriaId)
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("APIEndPoints", "ObtenerSubCategoria");
                using var cliente = ObtenerClienteConToken();

                var respuesta = await cliente.GetAsync($"{endpoint}?categoriaId={categoriaId}");
                respuesta.EnsureSuccessStatusCode();
                
                var json = await respuesta.Content.ReadAsStringAsync();
                var subCategorias = JsonSerializer.Deserialize<List<JsonElement>>(json);
                
                var resultado = subCategorias?.Select(s => new SelectListItem
                {
                    Value = s.GetProperty("id").GetString() ?? "",
                    Text = s.GetProperty("nombre").GetString() ?? ""
                }).ToList() ?? new List<SelectListItem>();
                
                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerSubCategorias: {ex.Message}");
                return new JsonResult(new { error = ex.Message });
            }
        }

        private async Task CargarCategorias()
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("APIEndPoints", "ObtenerCategorias");
                System.Diagnostics.Debug.WriteLine($"Endpoint de categorías: {endpoint}");
                
                using var cliente = ObtenerClienteConToken();

                var respuesta = await cliente.GetAsync(endpoint);
                System.Diagnostics.Debug.WriteLine($"Status Code: {respuesta.StatusCode}");
                
                if (!respuesta.IsSuccessStatusCode)
                {
                    var errorContent = await respuesta.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Respuesta de error: {errorContent}");
                }
                
                respuesta.EnsureSuccessStatusCode();

                var json = await respuesta.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"JSON recibido: {json}");
                
                var categorias = JsonSerializer.Deserialize<List<JsonElement>>(json);

                Categoria = categorias?.Select(c => new SelectListItem
                {
                    Value = c.GetProperty("id").GetString() ?? "",
                    Text = c.GetProperty("nombre").GetString() ?? ""
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando categorías: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                Categoria = new List<SelectListItem>();
            }
        }

        private HttpClient ObtenerClienteConToken()
        {
            var tokenClaim = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "AccessToken");
            var cliente = new HttpClient();
            
            cliente.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            if (tokenClaim != null)
                cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", tokenClaim.Value);
            
            return cliente;
        }
    }
}