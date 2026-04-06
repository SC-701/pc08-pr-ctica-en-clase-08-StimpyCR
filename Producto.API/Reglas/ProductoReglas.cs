using Abstracciones.Interfaces.Reglas;
using Abstracciones.Interfaces.Servicios;

namespace Reglas
{
    public class ProductoReglas : IProductoReglas
    {
        private readonly ITipoCambioDolar _tipoCambioServicio;

        public ProductoReglas(ITipoCambioDolar tipoCambioServicio)
        {
            _tipoCambioServicio = tipoCambioServicio;
        }

        public async Task<double> CalcularPrecioUSD(decimal precioColones, DateTime? fecha = null)
        {
            var fechaConsulta = (fecha ?? DateTime.UtcNow).ToString("yyyy-MM-dd");
            try
            {
                var tipoCambio = await _tipoCambioServicio.ObtenerTipoCambioDolar(fechaConsulta);
                if (tipoCambio.valorDatoPorPeriodo <= 0)
                    return 0d;

                var usd = (double)(precioColones / (decimal)tipoCambio.valorDatoPorPeriodo);
                return Math.Round(usd, 4);
            }
            catch
            {
                return 0d;
            }
        }
    }
}