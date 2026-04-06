
using Abstracciones.Modelos;
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
    public class EditarModel : PageModel
    {
        private IConfiguracion _configuracion;
        [BindProperty]
        public ProductoResponse Producto { get; set; } = default!;
        public ProductoRequest ProductoRequest { get; set; } = default!;
        [BindProperty]
        public List<SelectListItem> categorias { get; set; } = default!;
        [BindProperty]
        public List<SelectListItem> subCategorias { get; set; } = default!;
        [BindProperty]
        public Guid categoriaSeleccionada { get; set; } = default!;
        [BindProperty]
        public Guid subCategoriaSeleccionada { get; set; } = default!;

        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<ActionResult> OnGet(Guid? id)
        {
            if (id == null)
                return NotFound();

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerProducto");
            using var cliente = ObtenerClienteConToken();

            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, id));
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            if (respuesta.StatusCode == HttpStatusCode.OK)
            {
                await ObtenerCategoriasAsync();
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Producto = JsonSerializer.Deserialize<ProductoResponse>(resultado, opciones);

                if (Producto != null)
                {
                    await CargarSubCategoriasDelProducto();
                }
            }
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            if (Producto.Id == Guid.Empty)
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "EditarProducto");
            using var cliente = ObtenerClienteConToken();

            var solicitudProducto = new ProductoRequest
            {
                Nombre = Producto.Nombre,
                Descripcion = Producto.Descripcion,
                Precio = Producto.Precio,
                Stock = Producto.Stock,
                CodigoBarras = Producto.CodigoBarras,
                IdSubCategoria = subCategoriaSeleccionada
            };

            var respuesta = await cliente.PutAsJsonAsync(string.Format(endpoint, Producto.Id.ToString()), solicitudProducto);
            respuesta.EnsureSuccessStatusCode();
            return RedirectToPage("./Index");
        }

        private async Task ObtenerCategoriasAsync()
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCategorias");
                using var cliente = ObtenerClienteConToken();
                var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var respuesta = await cliente.SendAsync(solicitud);
                respuesta.EnsureSuccessStatusCode();

                if (respuesta.StatusCode == HttpStatusCode.OK)
                {
                    var resultado = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var resultadoDeserializado = JsonSerializer.Deserialize<List<dynamic>>(resultado, opciones);

                    categorias = resultadoDeserializado?.Select(a =>
                        new SelectListItem
                        {
                            Value = a.GetProperty("id").GetString(),
                            Text = a.GetProperty("nombre").GetString()
                        }).ToList() ?? new List<SelectListItem>();
                }
            }
            catch (Exception)
            {
                categorias = new List<SelectListItem>();
            }
        }

        private async Task CargarSubCategoriasDelProducto()
        {
            if (Producto?.Categoria != null)
            {
                var categoriaDelProducto = categorias?.FirstOrDefault(c => c.Text == Producto.Categoria);
                if (categoriaDelProducto != null && Guid.TryParse(categoriaDelProducto.Value, out Guid categoriaId))
                {
                    categoriaSeleccionada = categoriaId;
                    var subCategoriasList = await ObtenerSubCategoriasAsync(categoriaId);

                    subCategorias = subCategoriasList?.Select(sc =>
                        new SelectListItem
                        {
                            Value = sc.GetProperty("id").GetString(),
                            Text = sc.GetProperty("nombre").GetString(),
                            Selected = sc.GetProperty("nombre").GetString() == Producto.SubCategoria
                        }).ToList() ?? new List<SelectListItem>();

                    var subCategoriaSeleccionada = subCategorias.FirstOrDefault(sc => sc.Selected);
                    if (subCategoriaSeleccionada != null && Guid.TryParse(subCategoriaSeleccionada.Value, out Guid subCategoriaId))
                    {
                        this.subCategoriaSeleccionada = subCategoriaId;
                    }
                }
            }
        }

        public async Task<JsonResult> OnGetObtenerSubCategorias(Guid categoriaId)
        {
            var subCategorias = await ObtenerSubCategoriasAsync(categoriaId);
            return new JsonResult(subCategorias);
        }

        private async Task<List<JsonElement>> ObtenerSubCategoriasAsync(Guid categoriaId)
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerSubCategorias");
                using var cliente = ObtenerClienteConToken();
                var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, categoriaId));

                var respuesta = await cliente.SendAsync(solicitud);
                respuesta.EnsureSuccessStatusCode();

                if (respuesta.StatusCode == HttpStatusCode.OK)
                {
                    var resultado = await respuesta.Content.ReadAsStringAsync();
                    var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<JsonElement>>(resultado, opciones) ?? new List<JsonElement>();
                }
                return new List<JsonElement>();
            }
            catch (Exception)
            {
                return new List<JsonElement>();
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
