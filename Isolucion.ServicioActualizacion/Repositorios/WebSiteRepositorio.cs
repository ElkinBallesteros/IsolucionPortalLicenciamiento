using Isolucion.ServicioActualizacion.Modelo;
using System.Data.Entity;
using System.Linq;

namespace Isolucion.ServicioActualizacion.Repositorios
{
   public class WebSiteRepositorio : IWebSiteRepositorio
   {
      private readonly ActualizacionContexto _contexto;
      public WebSiteRepositorio(ActualizacionContexto contexto)
      {
         _contexto = contexto;
      }

      public WEBSITE ObtenerInformacionWebsite()
      {
         return _contexto.WEBSITE.FirstOrDefault();
      }

      public void ActualizarWebsite()
      {
         var webSite = _contexto.WEBSITE.FirstOrDefault();
         webSite.RequiereActualizacion = false;
         _contexto.Entry(webSite).State = EntityState.Modified;

         _contexto.SaveChanges();
      }

      public string ObtenerSQLVersion()
      {
         return _contexto.SP_ObtenerSQLVersion().FirstOrDefault();
      }
   }
}
