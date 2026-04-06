namespace Abstracciones.Interfaces.Reglas
{
    public interface IProductoReglas
    {
        Task<double> CalcularPrecioUSD(decimal precioColones, DateTime? fecha = null);
    }
}