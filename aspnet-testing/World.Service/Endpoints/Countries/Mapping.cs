using AutoMapper;
using World.Service.Database.Countries;

namespace World.Service.Endpoints.Countries;

public class CountryMapping : Profile
{
    public CountryMapping()
    {
        CreateMap<CountryEntity, CountryDto>();
    }
}
