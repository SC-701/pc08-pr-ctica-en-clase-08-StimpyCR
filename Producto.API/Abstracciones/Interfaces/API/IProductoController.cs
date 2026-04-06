using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Abstracciones.Modelos.ProductoBase;

namespace Abstracciones.Interfaces.API
{
    public interface IProductoController
    {
        Task<IEnumerable<ProductoResponse>> Obtener();
        Task<ProductoResponse> Obtener(Guid Id);
        Task<Guid> Agregar(ProductoRequest producto);
        Task<Guid> Editar(Guid Id, ProductoRequest producto);
        Task<Guid> Eliminar(Guid Id);
    }
}
