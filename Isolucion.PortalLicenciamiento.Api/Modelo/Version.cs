//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Isolucion.PortalLicenciamiento.Api.Modelo
{
    using System;
    using System.Collections.Generic;
    
    public partial class Version
    {
        public int Id_Version { get; set; }
        public string NomVersion { get; set; }
        public string Descripcion { get; set; }
        public System.DateTime Fecha { get; set; }
        public bool Activo { get; set; }
        public string RutaDescarga { get; set; }
        public int Id_WebSite { get; set; }
    }
}
