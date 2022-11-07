using System.Configuration;

namespace Isolucion.ServicioActualizacion.Common
{
   internal class ConstantesGlobales
   {
      public static bool EnEjecucion { get; set; }
      public static string PortalLicenciamientoApiUrl => ConfigurationManager.AppSettings["PortalLicenciamientoApiUrl"];
      public static bool DescargaOnline => ConfigurationManager.AppSettings["DescargaOnline"].StringToBoolean();
      public static string[] Aplicaciones => ConfigurationManager.AppSettings["Aplicaciones"].Split(',');
      public static string poolEstandar => ConfigurationManager.AppSettings["poolEstandar"];
      public static string poolEstandarTEST => ConfigurationManager.AppSettings["poolEstandarTEST"];
      public static string poolApiDataConnector => ConfigurationManager.AppSettings["poolApiDataConnector"];
      public static string poolApiRiesgosDafp => ConfigurationManager.AppSettings["poolApiRiesgosDafp"];
      public static string poolApGestionCamio => ConfigurationManager.AppSettings["poolApGestionCamio"];
      public static string EstandarWebNombres => ConfigurationManager.AppSettings["EstandarWebNombres"];
      public static string EstandarTestWebNombres => ConfigurationManager.AppSettings["EstandarTestWebNombres"];
      public static string ApiDataConectorWebNombres => ConfigurationManager.AppSettings["ApiDataConectorWebNombres"];
      public static string ApiRiesgoDafpNombres => ConfigurationManager.AppSettings["ApiRiesgoDafpNombres"];
      public static string ApiGestionDelCambioNombres => ConfigurationManager.AppSettings["ApiGestionDelCambioNombres"];
      public static string IsolucionServicioNombre => ConfigurationManager.AppSettings["IsolucionServicioNombre"];
      public static string xProjectAnalyzerNombre => ConfigurationManager.AppSettings["xProjectAnalyzerNombre"];
      public static string GenericEventHandler => ConfigurationManager.AppSettings["GenericEventHandler"];
   }
}
