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
    
    public partial class ActualizacionLog
    {
        public int Id_ActualizacionLog { get; set; }
        public string Mensaje { get; set; }
        public string Stack_Tarace { get; set; }
        public System.DateTime Fechas { get; set; }
        public Nullable<int> CodCliente { get; set; }
        public string Estado { get; set; }
    }
}
