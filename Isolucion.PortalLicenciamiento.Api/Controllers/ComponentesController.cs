using Isolucion.PortalLicenciamiento.Api.Modelo;
using Isolucion.PortalLicenciamiento.Api.Modelo.Dto;
using Isolucion.PortalLicenciamiento.Api.Servicios;
using System.Web.Http;

namespace Isolucion.PortalLicenciamiento.Api.Controllers
{
   [Route("Api/Componentes")]
   public class ComponentesController : ApiController
   {
      private readonly IComponenteServicio _servicio;
      public ComponentesController(IComponenteServicio servicio)
      {
         _servicio = servicio;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      [HttpGet]

      [Route("ObtenerVersion")]
      public IHttpActionResult ObtenerVersion([FromUri] int idWebSite)
      {
         return Ok(_servicio.ObtenerVersion(idWebSite));
      }

      /// <summary>
      /// Inserta un nuevo registro en el log de transacciones.
      /// </summary>
      /// <param name="log">The log.</param>
      /// <returns></returns>
      [HttpPost]

      [Route("LogProceso")]
      public IHttpActionResult LogProceso([FromBody] ActualizacionLogDto log)
      {
         return Ok(_servicio.LogProcesoObtenerComponentes(log));
      }

      [HttpPost]

      [Route("GuardarInformacionCliente")]

      public IHttpActionResult GuardarInformacionCliente(InformacionCliente info)
      {
         _servicio.GuardarInformacionCliente(info);
         return Ok();
      }
   }
}
