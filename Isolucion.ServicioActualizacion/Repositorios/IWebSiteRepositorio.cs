using Isolucion.ServicioActualizacion.Modelo;

namespace Isolucion.ServicioActualizacion.Repositorios
{
   public interface IWebSiteRepositorio
   {
      WEBSITE ObtenerInformacionWebsite();
      void ActualizarWebsite();
      string ObtenerSQLVersion();
   }
}