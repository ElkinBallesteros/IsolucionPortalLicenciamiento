//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Isolucion.ServicioActualizacion.Modelo
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class ActualizacionContexto : DbContext
    {
        public ActualizacionContexto()
            : base("name=ActualizacionContexto")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<WEBSITE> WEBSITE { get; set; }
    
        public virtual ObjectResult<string> SP_ObtenerSQLVersion()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("SP_ObtenerSQLVersion");
        }
    }
}
