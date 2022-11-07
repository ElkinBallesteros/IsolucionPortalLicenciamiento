using System;

namespace Isolucion.PortalLicenciamiento.Api.Modelo.Dto
{
    public class VersionDto
    {
        public int Id_Version { get; set; }

        public string NomVersion { get; set; }

        public string Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public Boolean Activo { get; set; }

        public string RutaDescarga { get; set; }
    }
}