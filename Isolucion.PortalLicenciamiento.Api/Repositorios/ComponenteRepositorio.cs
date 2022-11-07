using Isolucion.PortalLicenciamiento.Api.Enumeradores;
using Isolucion.PortalLicenciamiento.Api.Modelo;
using System.Data.Entity;
using System.Linq;

namespace Isolucion.PortalLicenciamiento.Api.Repositorios
{
   public class ComponenteRepositorio : IComponenteRepositorio
   {
      private readonly RepositorioDbContext _context;

      public ComponenteRepositorio(RepositorioDbContext context)
      {
         _context = context;
      }

      public Modelo.Version ObtenerVersion(int idWebSite)
      {
         return _context.Version.FirstOrDefault(v => v.Id_WebSite == idWebSite);
      }

      public int LogProcesoObtenerComponentes(ActualizacionLog log)
      {
         var cliente = _context.Cliente.Find(log.CodCliente);

         if (cliente != null)
         {
            switch (log.Estado)
            {
               case "Exitoso":
                  cliente.ActualizacionStatusMensaje = log.Estado;
                  break;
               case "Fallido":
                  cliente.ActualizacionStatusMensaje = log.Mensaje;
                  break;
            }

            cliente.ActualizacionStatusMensaje = log.Mensaje;
            _context.Entry(cliente).State = EntityState.Modified;
         }
         
         _context.ActualizacionLog.Add(log);
         _context.SaveChanges();
         return log.Id_ActualizacionLog;
      }

      public void GuardarInformacionCliente(InformacionCliente info)
      {
         var infoCliente = _context.InformacionCliente.FirstOrDefault(i => i.CodCliente == info.CodCliente);

         if (infoCliente == null)
         {
            _context.InformacionCliente.Add(info);
         }
         else
         {
            infoCliente.InformacionClienteDetalle = info.InformacionClienteDetalle;
            _context.Entry(infoCliente).State = EntityState.Modified;
         }

         _context.SaveChanges();
      }
   }
}