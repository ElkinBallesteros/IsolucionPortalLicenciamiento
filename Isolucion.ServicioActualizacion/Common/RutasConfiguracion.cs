using System.Configuration;

namespace Isolucion.ServicioActualizacion.Common
{
    internal class RutasConfiguracion
    {
        public static string PatDescarga => ConfigurationManager.AppSettings["pathfile"];
        public static string PathEstandar => ConfigurationManager.AppSettings["PathEstandar"];
        public static string PathEstandarTEST => ConfigurationManager.AppSettings["PathEstandarTEST"];
        public static string PathApi => ConfigurationManager.AppSettings["PathApi"];
        public static string PathApiRiesgoDafp => ConfigurationManager.AppSettings["PathApiRiesgoDafp"];
        public static string PathApiGestionCambio => ConfigurationManager.AppSettings["PathApiGestionCambio"];
        public static string PathServicio => ConfigurationManager.AppSettings["PathServicio"];
        public static string PathGenericHandler => ConfigurationManager.AppSettings["PathGenericHandler"];
        public static string PathxProjectAnalyzer => ConfigurationManager.AppSettings["PatxProjectAnalyzer"];
        public static string PathIndexador => ConfigurationManager.AppSettings["PathIndexador"];
        public static string PathDescargaLocal => ConfigurationManager.AppSettings["PathDescargaLocal"];
        public static string Backup => ConfigurationManager.AppSettings["PathBackup"];
    }
}
