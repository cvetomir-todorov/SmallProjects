using AutoMapper;
using World.Service.Database.Continents;

namespace World.Service.Endpoints.Continents;

public class ContinentMapping : Profile
{
    public ContinentMapping()
    {
        CreateMap<ContinentEntity, ContinentDto>();
    }
}