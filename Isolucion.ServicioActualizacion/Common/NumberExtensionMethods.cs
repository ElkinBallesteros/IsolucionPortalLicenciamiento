namespace Isolucion.ServicioActualizacion.Common
{
    public static class NumberExtensionMethods
    {
        public static long StingToLong(this string value)
        {
            if (!long.TryParse(value, out long number))
                return 0;

            return number;
        }
    }
}
