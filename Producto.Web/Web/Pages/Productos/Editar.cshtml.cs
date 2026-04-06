using Abstracciones.Modelos;
using Abstracciones.Reglas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;

namespace Web.Pages.Productos
{
    [Authorize]
    public class EditarModel : PageModel
    {
        private IConfiguracion _configuracion;

        [BindProperty]
        public ProductoResponse producto { get; set; } = default!;

        [BindProperty]
        public List<SelectListItem> categorias { get; set; } = new();

        [BindProperty]
        public List<SelectListItem> subCategorias { get; set; } = new();

        [BindProperty]
        public Guid categoriaSeleccionada { get; set; }

        [BindProperty]
        public Guid subCategoriaSeleccionada { get; set; }

        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task<ActionResult> OnGet(Guid? id)
        {
            if (id == null)
                return NotFound();

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerProductoPorId");

            using var cliente = ObtenerClienteConToken();
            var respuesta = await cliente.GetAsync(string.Format(endpoint, id));

            respuesta.EnsureSuccessStatusCode();

            var json = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            producto = JsonSerializer.Deserialize<ProductoResponse>(json, opciones)!;

            if (producto != null)
            {
                // 🔥 PRIMERO asignar IDs
                categoriaSeleccionada = producto.IdCategoria;
                subCategoriaSeleccionada = producto.IdSubCategoria;

                // 🔥 luego cargar categorías
                await ObtenerCategoriasAsync();

                // 🔥 luego cargar subcategorías de esa categoría
                var listaSub = await ObtenerSubCategoriasAsync(categoriaSeleccionada);

                subCategorias = listaSub.Select(a =>
                    new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre,
                        Selected = a.Id == producto.IdSubCategoria
                    }).ToList();
            }

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            if (producto.Id == Guid.Empty)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await ObtenerCategoriasAsync();
                subCategorias = (await ObtenerSubCategoriasAsync(categoriaSeleccionada))
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre
                    }).ToList();

                return Page();
            }

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "EditarProducto");

            using var cliente = ObtenerClienteConToken();

            var solicitudProducto = new ProductoRequest
            {
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                CodigoBarras = producto.CodigoBarras,
                IdSubCategoria = subCategoriaSeleccionada
            };

            var respuesta = await cliente.PutAsJsonAsync(string.Format(endpoint, producto.Id), solicitudProducto);

            respuesta.EnsureSuccessStatusCode();

            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetObtenerSubCategorias(Guid categoriaId)
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerSubCategoria");
                string url = $"{endpoint}?categoriaId={categoriaId}";

                using var cliente = ObtenerClienteConToken();
                var respuesta = await cliente.GetAsync(url);

                respuesta.EnsureSuccessStatusCode();

                var json = await respuesta.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<JsonElement>>(json);

                var resultado = lista?.Select(x => new SelectListItem
                {
                    Value = x.GetProperty("id").GetString(),
                    Text = x.GetProperty("nombre").GetString()
                }).ToList();

                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        private async Task ObtenerCategoriasAsync()
        {
            try
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerCategorias");

                using var cliente = ObtenerClienteConToken();
                var respuesta = await cliente.GetAsync(endpoint);

                respuesta.EnsureSuccessStatusCode();

                var json = await respuesta.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<JsonElement>>(json);

                categorias = lista?.Select(x => new SelectListItem
                {
                    Value = x.GetProperty("id").GetString(),
                    Text = x.GetProperty("nombre").GetString(),
                    Selected = x.GetProperty("id").GetString() == categoriaSeleccionada.ToString()
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                categorias = new List<SelectListItem>();
            }
        }

        private async Task<List<(Guid Id, string Nombre)>> ObtenerSubCategoriasAsync(Guid categoriaId)
        {
            var resultado = new List<(Guid, string)>();

            try
            {
                string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerSubCategoria");
                string url = $"{endpoint}?categoriaId={categoriaId}";

                using var cliente = ObtenerClienteConToken();
                var respuesta = await cliente.GetAsync(url);

                respuesta.EnsureSuccessStatusCode();

                var json = await respuesta.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<JsonElement>>(json);

                foreach (var item in lista!)
                {
                    var id = item.GetProperty("id").GetString();
                    var nombre = item.GetProperty("nombre").GetString();

                    if (Guid.TryParse(id, out var guid))
                    {
                        resultado.Add((guid, nombre!));
                    }
                }
            }
            catch { }

            return resultado;
        }

        private HttpClient ObtenerClienteConToken()
        {
            var token = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "AccessToken");

            var cliente = new HttpClient();

            if (token != null)
            {
                cliente.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);
            }

            return cliente;
        }
    }
}