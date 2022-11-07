using Isolucion.PortalLicenciamiento.Api;
using Swashbuckle.Application;
using System.Web.Http;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Isolucion.PortalLicenciamiento.Api
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
               .EnableSwagger("docs/{apiVersion}/swagger", c =>
               {
                   c.SingleApiVersion("v1", "Isolucion.PortalLicenciamiento.Api");
                   c.IncludeXmlComments($"{System.AppDomain.CurrentDomain.BaseDirectory}\\bin\\Isolucion.PortalLicenciamiento.Api.xml");
               })
               .EnableSwaggerUi();
        }
    }
}
