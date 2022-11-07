using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using Isolucion.PortalLicenciamiento.Api.Modelo;
using Isolucion.PortalLicenciamiento.Api.Repositorios;
using Isolucion.PortalLicenciamiento.Api.Servicios;
using System.Reflection;
using System.Web.Http;

namespace Isolucion.PortalLicenciamiento.Api
{
    public static class Configurator
    {
        public static void Configure()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;
            var mapperConfig = AutoMapperConfiguration.Configure();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly())
               .InstancePerRequest();

            //build the mapper
            IMapper mapper = mapperConfig.CreateMapper();

            builder.RegisterInstance(mapper);

            builder.Register(c => new RepositorioDbContext())
               .As<RepositorioDbContext>()
               .InstancePerLifetimeScope();

            builder.RegisterType<ComponenteRepositorio>()
               .PropertiesAutowired().As<IComponenteRepositorio>();

            builder.RegisterType<ComponenteServicio>()
               .PropertiesAutowired().As<IComponenteServicio>();

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}