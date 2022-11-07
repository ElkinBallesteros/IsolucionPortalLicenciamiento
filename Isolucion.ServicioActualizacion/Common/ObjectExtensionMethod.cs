using Newtonsoft.Json;

namespace Isolucion.ServicioActualizacion.Common
{
    internal static class ObjectExtensionMethod
    {
        public static T DeserializeStringToObject<T>(this string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default;
            }
        }

        public static bool StringToBoolean(this string value)
        {
            bool.TryParse(value, out bool retValue);

            return retValue;
        }

        public static string BooleanToString(this bool value)
        {
            return value ? "Si" : "No";
        }
    }
}
