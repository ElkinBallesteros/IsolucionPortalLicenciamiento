namespace Isolucion.PortalLicenciamiento.Api.Modelo.Dto
{
    public class ComponenteDto
    {
        public int Id_Componente { get; set; }

        public string NomComponente { get; set; }

        public string Descripcion { get; set; }

        public int TipoComponente { get; set; }

        public string RutasRelativas { get; set; }

        public string Excepciones { get; set; }

        public string NombresSistema { get; set; }

        public string NombresAuxiliares { get; set; }

        public string NombresPool { get; set; }

        public string NombreArchivoZip { get; set; }

        public int ID_WebSite { get; set; }
    }
}