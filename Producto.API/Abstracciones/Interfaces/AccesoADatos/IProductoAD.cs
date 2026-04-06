using Abstracciones.Modelos;

namespace Abstracciones.Interfaces.AccesoADatos
{
    public interface IProductoAD
    {
        Task<IEnumerable<ProductoResponse>> Obtener();
        Task<ProductoDetalle> Obtener(Guid Id);
        Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerCategorias();
        Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerSubcategorias(Guid categoriaId);
        Task<Guid> Agregar(ProductoRequest producto);
        Task<Guid> Editar(Guid Id, ProductoRequest producto);
        Task<Guid> Eliminar(Guid Id);
    }
}
