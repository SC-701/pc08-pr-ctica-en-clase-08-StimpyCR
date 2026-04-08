using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Abstracciones.Modelos.ProductoBase;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly ILogger<ProductoController> _logger;
        private readonly IProductoFlujo _productoFlujo;

        public ProductoController(
            ILogger<ProductoController> logger,
            IProductoFlujo productoFlujo)
        {
            _logger = logger;
            _productoFlujo = productoFlujo;
        }


        // GET: api/<ProductoController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var resultado = await _productoFlujo.Obtener();
            return Ok(resultado);
        }

        // GET api/<ProductoController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(Guid id)
        {
            try
            {
                var resultado = await _productoFlujo.Obtener(id);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("BCCR 401"))
            {
                return StatusCode(StatusCodes.Status502BadGateway, "No se pudo obtener el tipo de cambio del BCCR (401). Verifique el token.");
            }
        }

        // GET api/<ProductoController>/categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var resultado = await _productoFlujo.ObtenerCategorias();

            if (!resultado.Any())
            {
                return NoContent();
            }

            var respuesta = resultado.Select(x => new
            {
                Id = x.Id,
                Nombre = x.Nombre
            });

            return Ok(respuesta);
        }

        // GET api/<ProductoController>/subcategorias
        [HttpGet("subcategorias")]
        public async Task<IActionResult> ObtenerSubcategorias([FromQuery] Guid categoriaId)
        {
            if (categoriaId == Guid.Empty)
            {
                return BadRequest("categoriaId es requerido");
            }

            var resultado = await _productoFlujo.ObtenerSubcategorias(categoriaId);

            if (!resultado.Any())
            {
                return NoContent();
            }

            var respuesta = resultado.Select(x => new
            {
                Id = x.Id,
                Nombre = x.Nombre
            });

            return Ok(respuesta);
        }

        // POST api/<ProductoController>
        [HttpPost]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Agregar([FromBody] ProductoRequest producto)
        {
            var resultado = await _productoFlujo.Agregar(producto);
            return Ok(resultado);
        }



        // PUT api/<ProductoController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Editar(Guid id,[FromBody] ProductoRequest producto)
        {
            var resultado = await _productoFlujo.Editar(id, producto);
            return Ok(resultado);
        }

        // DELETE api/<ProductoController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var resultado = await _productoFlujo.Eliminar(id);  
            return Ok(resultado);
        }
    }
}
