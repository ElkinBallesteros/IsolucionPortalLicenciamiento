using Isolucion.ServicioActualizacion.Modelo;

namespace Isolucion.ServicioActualizacion.Servicios
{
    public interface IWebSiteServicio
    {
       WEBSITE ObtenerInformacionWebsite();
        void ActualizarWebsite();
    }
}