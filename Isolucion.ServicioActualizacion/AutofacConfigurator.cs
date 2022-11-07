using Autofac;
using Isolucion.ServicioActualizacion.Modelo;
using Isolucion.ServicioActualizacion.Negocio;
using Isolucion.ServicioActualizacion.Repositorios;
using Isolucion.ServicioActualizacion.Servicios;


namespace Isolucion.ServicioActualizacion
{
   internal class AutofacConfigurator
   {
      public static IContainer GetContainer()
      {
         var builder = new ContainerBuilder();
         builder.Register(c => new ActualizacionContexto()).As<ActualizacionContexto>().InstancePerLifetimeScope();
         builder.RegisterType<WebSiteRepositorio>().As<IWebSiteRepositorio>().InstancePerDependency();
         builder.RegisterType<WebSiteServicio>().As<IWebSiteServicio>().InstancePerDependency();

         builder.RegisterType<ServicioActualizacionNegocio>().As<IServicioActualizacionNegocio>()
            .InstancePerDependency();

         
         return builder.Build();

      }
   }
}
