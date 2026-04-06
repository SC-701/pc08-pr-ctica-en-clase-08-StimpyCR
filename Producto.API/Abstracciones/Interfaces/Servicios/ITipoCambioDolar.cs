using Abstracciones.Modelos.Servicios.CambioDolar;

namespace Abstracciones.Interfaces.Servicios
{
    public interface ITipoCambioDolar
    {
        Task<CambioDolar> ObtenerTipoCambioDolar(string fecha);
    }
}
