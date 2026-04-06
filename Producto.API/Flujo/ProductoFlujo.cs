using Abstracciones.Interfaces.AccesoADatos;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;

namespace Flujo
{
    public class ProductoFlujo : IProductoFlujo
    {
        private readonly IProductoAD _productoAD;
        private readonly IProductoReglas _productoReglas;

        public ProductoFlujo(IProductoAD productoAD, IProductoReglas productoReglas)
        {
            _productoAD = productoAD;
            _productoReglas = productoReglas;
        }

        public Task<Guid> Agregar(ProductoRequest producto)
        {
            return _productoAD.Agregar(producto);
        }

        public Task<Guid> Editar(Guid Id, ProductoRequest producto)
        {
            return _productoAD.Editar(Id, producto);
        }

        public Task<Guid> Eliminar(Guid Id)
        {
            return _productoAD.Eliminar(Id);
        }

        public Task<IEnumerable<ProductoResponse>> Obtener()
        {
            return _productoAD.Obtener();
        }

        public async Task<ProductoDetalle> Obtener(Guid Id)
        {
            var detalle = await _productoAD.Obtener(Id);

            var precioUSD = await _productoReglas.CalcularPrecioUSD(detalle.Precio, DateTime.UtcNow);
            detalle.PrecioDolar = precioUSD;
            detalle.PrecioUSD = precioUSD;
            detalle.fechaActual = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return detalle;
        }

        public Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerCategorias()
        {
            return _productoAD.ObtenerCategorias();
        }

        public Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerSubcategorias(Guid categoriaId)
        {
            return _productoAD.ObtenerSubcategorias(categoriaId);
        }
    }
}