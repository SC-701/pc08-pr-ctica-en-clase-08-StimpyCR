using Microsoft.Data.SqlClient;


namespace Abstracciones.Interfaces.AccesoADatos
{
    public interface IRepositorioDapper
    {
        SqlConnection ObtenerRepositorio();
    }
}
