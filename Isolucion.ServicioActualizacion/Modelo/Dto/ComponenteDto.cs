namespace Isolucion.ServicioActualizacion.Modelo
{
    public class ComponenteDto
    {
        public int Id_Componente { get; set; }
        public string NomComponente { get; set; }
        public int TipoComponente { get; set; }
        public string Excepciones { get; set; }
        public string NombresSistema { get; set; }
        public string NombresAuxiliares { get; set; }
        public string NombresPool { get; set; }
        public string NombreArchivoZip { get; set; }
        public int Id_WebSite { get; set; }
        public string NombreServicio { get; set; }
    }
}
