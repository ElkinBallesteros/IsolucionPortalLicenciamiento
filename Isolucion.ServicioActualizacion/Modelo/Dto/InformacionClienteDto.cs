namespace Isolucion.PortalLicenciamiento.Api.Modelo.Dto
{
    public class InformacionClienteDto
    {
        public int Id_InformacionCliente { get; set; }
        public int CodCliente { get; set; }
        public string InformacionClienteDetalle { get; set; }
    }

    public class DetalleInformacionClienteDto
    {
        public int CodCliente { get; set; }
        public string OSVersion { get; set; }
        public string OSPlatoform { get; set; }
        public string OSServicePack { get; set; }
        public string VersionString { get; set; }
        public string SQLVersion { get; set; }
        public string SmartFlowLicence { get; set; }
        public string SIFXLicence { get; set; }
    }
}