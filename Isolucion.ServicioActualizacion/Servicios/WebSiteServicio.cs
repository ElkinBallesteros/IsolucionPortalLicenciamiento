using Isolucion.ServicioActualizacion.Modelo;
using Isolucion.ServicioActualizacion.Repositorios;

namespace Isolucion.ServicioActualizacion.Servicios
{
    public class WebSiteServicio : IWebSiteServicio
    {
        private readonly IWebSiteRepositorio _repo;

        public WebSiteServicio(IWebSiteRepositorio repo)
        {
            _repo = repo;
        }

        public WEBSITE ObtenerInformacionWebsite()
        {
            return _repo.ObtenerInformacionWebsite();
        }

        public void ActualizarWebsite()
        {
            _repo.ActualizarWebsite();
        }
    }
}
