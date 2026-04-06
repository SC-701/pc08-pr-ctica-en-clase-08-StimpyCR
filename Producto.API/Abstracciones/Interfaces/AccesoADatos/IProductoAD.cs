using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Abstracciones.Modelos.ProductoBase;

namespace Abstracciones.Interfaces.AccesoADatos
{
    public interface IProductoAD
    {
        Task<IEnumerable<ProductoResponse>> Obtener();
        Task<ProductoDetalle> Obtener(Guid Id);
        Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerCategorias();
        Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerSubcategorias();
        Task<Guid> Agregar(ProductoRequest producto);
        Task<Guid> Editar(Guid Id, ProductoRequest producto);
        Task<Guid> Eliminar(Guid Id);
    }
}
