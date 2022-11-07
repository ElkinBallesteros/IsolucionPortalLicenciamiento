using System;

namespace Isolucion.ServicioActualizacion.Modelo
{
    public class ActualizacionLogDto
    {
        public int Id_ActualizacionLog { get; set; }
        public string Mensaje { get; set; }
        public string Stack_Tarace { get; set; }
        public System.DateTime Fechas { get; set; }
        public Nullable<int> CodCliente { get; set; }
        public string Estado { get; set; }
    }
}