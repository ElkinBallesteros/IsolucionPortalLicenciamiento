using AutoMapper;
using Isolucion.PortalLicenciamiento.Api.Modelo;
using Isolucion.PortalLicenciamiento.Api.Modelo.Dto;

namespace Isolucion.PortalLicenciamiento.Api
{
    public class AutoMapperConfiguration
    {
        public static MapperConfiguration Configure()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VersionDto, Modelo.Version>();
                cfg.CreateMap<Modelo.Version, VersionDto>();
                cfg.CreateMap<ComponenteDto, Componente>();
                cfg.CreateMap<Componente, ComponenteDto>();
                cfg.CreateMap<ActualizacionLog, ActualizacionLogDto>();
                cfg.CreateMap<ActualizacionLogDto, ActualizacionLog>();

            });

            return mapperConfiguration;
        }
    }
}