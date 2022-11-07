using Isolucion.PortalLicenciamiento.Api.Modelo;

namespace Isolucion.PortalLicenciamiento.Api.Repositorios
{
   public interface IComponenteRepositorio
   {
      Version ObtenerVersion(int idWebSite);
      int LogProcesoObtenerComponentes(ActualizacionLog log);
      void GuardarInformacionCliente(InformacionCliente info);
   }
}