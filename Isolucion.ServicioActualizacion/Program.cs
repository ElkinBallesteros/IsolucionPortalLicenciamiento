using Topshelf;

namespace Isolucion.ServicioActualizacion
{
    internal class Program
    {
        static void Main()
        {
            StarService();
        }

        private static void StarService()
        {
            HostFactory.Run(x =>
            {
                x.Service<Service>(s =>
             {
                 s.ConstructUsing(name => new Service());
                 s.WhenStarted(tc => tc.Start());
                 s.WhenStopped(tc => tc.Stop());
             });

                x.RunAsLocalSystem();
                x.SetDescription("Isolucion.ServicioActualizacion");
                x.SetDisplayName("Isolucion.ServicioActualizacion");
                x.SetServiceName("Isolucion.ServicioActualizacion");
            });
        }
    }
}
