using Abstracciones.Interfaces.AccesoADatos;
using Abstracciones.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Abstracciones.Modelos.ProductoBase;

namespace AccesoADatos
{
    public class ProductoAD : IProductoAD
    {
        private readonly IRepositorioDapper _repositorioDapper;
        private readonly SqlConnection _sqlConnection;

        public ProductoAD(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorio();
        }

        public async Task<Guid> Agregar(ProductoRequest producto)
        {
            string query = "AgregarProducto";

            var parametros = new
            {
                Id = Guid.NewGuid(),
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                CodigoBarras = producto.CodigoBarras,
                IdSubCategoria = producto.IdSubCategoria
            };

            var resultadoConsulta = await _sqlConnection.ExecuteScalarAsync<Guid>(
                query,
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return resultadoConsulta;
        }

        public async Task<Guid> Editar(Guid Id, ProductoRequest producto)
        {
            await verificarProductoExistencia(Id);
            string query = @"EditarProducto";

            var resultadoConsulta = await _sqlConnection.ExecuteScalarAsync<Guid>(
                query,
                new
                {
                    Id = Id,
                    IdSubCategoria = producto.IdSubCategoria, 
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Stock = producto.Stock,
                    CodigoBarras = producto.CodigoBarras
                },
                commandType: CommandType.StoredProcedure
            );

            return resultadoConsulta;
        }

        public async Task<Guid> Eliminar(Guid Id)
        {
            await verificarProductoExistencia(Id);
            string query = @"EliminarProducto";

            var resultadoConsulta = await _sqlConnection.ExecuteScalarAsync<Guid>(query, new
            {
                Id = Id
            });

            return resultadoConsulta;
        }

        public async Task<IEnumerable<ProductoResponse>> Obtener()
        {
            string query = @"ObtenerProductos";
            var resultadoConsulta = await _sqlConnection.QueryAsync<ProductoResponse>(query, commandType: CommandType.StoredProcedure);
            return resultadoConsulta;
        }

        public async Task<ProductoDetalle> Obtener(Guid Id)
        {
            string query = @"ObtenerProducto";
            var resultadoConsulta = await _sqlConnection.QueryAsync<ProductoDetalle>(query, new { Id = Id }, commandType: CommandType.StoredProcedure);
            return resultadoConsulta.FirstOrDefault();
        }

        public async Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerCategorias()
        {
            using (var connection = _repositorioDapper.ObtenerRepositorio())
            {
                string query = @"ObtenerCategorias";

                var resultado = await connection.QueryAsync(
                    query,
                    commandType: CommandType.StoredProcedure
                );

                return resultado?.Select(x => ((Guid)x.Id, (string)x.Nombre)) ?? Enumerable.Empty<(Guid, string)>();
            }
        }

       
        public async Task<IEnumerable<(Guid Id, string Nombre)>> ObtenerSubcategorias(Guid categoriaId)
        {
            using (var connection = _repositorioDapper.ObtenerRepositorio())
            {
                string query = @"ObtenerSubcategorias";

                var resultado = await connection.QueryAsync(
                    query,
                    new { categoriaId = categoriaId },
                    commandType: CommandType.StoredProcedure
                );

                return resultado?.Select(x => ((Guid)x.Id, (string)x.Nombre)) ?? Enumerable.Empty<(Guid, string)>();
            }
        }

        private async Task verificarProductoExistencia(Guid id)
        {
            ProductoResponse? resultadoConsultaProducto = await Obtener(id);
            if (resultadoConsultaProducto == null)
            {
                throw new Exception("El producto que intenta editar no existe.");
            }
        }
    }
}