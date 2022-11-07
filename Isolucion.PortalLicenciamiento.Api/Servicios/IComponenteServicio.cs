using Isolucion.PortalLicenciamiento.Api.Modelo;
using Isolucion.PortalLicenciamiento.Api.Modelo.Dto;

namespace Isolucion.PortalLicenciamiento.Api.Servicios
{
   public interface IComponenteServicio
   {
      VersionDto ObtenerVersion(int idWebSite);
      int LogProcesoObtenerComponentes(ActualizacionLogDto log);
      void GuardarInformacionCliente(InformacionCliente info);
   }
}