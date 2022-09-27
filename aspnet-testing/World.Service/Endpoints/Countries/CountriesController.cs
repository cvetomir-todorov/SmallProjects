using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using World.Service.Database.Continents;
using World.Service.Database.Countries;

namespace World.Service.Endpoints.Countries;

[Route("api/countries")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IContinentRepo _continentRepo;
    private readonly ICountryRepo _countryRepo;

    public CountriesController(IMapper mapper, IContinentRepo continentRepo, ICountryRepo countryRepo)
    {
        _mapper = mapper;
        _continentRepo = continentRepo;
        _countryRepo = countryRepo;
    }

    [HttpGet]
    public async Task<ActionResult<CountriesResponse>> GetAllCountries()
    {
        IEnumerable<CountryEntity> allCountryEntities = await _countryRepo.GetAllCountries();
        CountryDto[] allCountryDtos = _mapper.Map<CountryDto[]>(allCountryEntities);

        CountriesResponse response = new();
        response.Countries = allCountryDtos;

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CountryDto>> GetSingleCountry([FromRoute] int id)
    {
        CountryEntity? countryEntity = await _countryRepo.GetSingleCountry(id);
        if (countryEntity == null)
        {
            return NotFound();
        }

        CountryDto countryDto = _mapper.Map<CountryDto>(countryEntity);

        return Ok(countryDto);
    }

    [HttpPost]
    public async Task<ActionResult<CountryDto>> AddCountry([FromBody] AddCountryRequest request)
    {
        IEnumerable<ContinentEntity> allContinents = await _continentRepo.GetAllContinents();
        ContinentEntity? continent = allContinents.FirstOrDefault(c => c.Name == request.ContinentName);
        if (continent == null)
        {
            return Conflict($"Unknown continent named '{request.ContinentName}'.");
        }

        CountryEntity countryEntity = await _countryRepo.AddCountry(request.CountryName, request.CountryPopulation, continent);
        CountryDto countryDto = _mapper.Map<CountryDto>(countryEntity);

        return Ok(countryDto);
    }
}
