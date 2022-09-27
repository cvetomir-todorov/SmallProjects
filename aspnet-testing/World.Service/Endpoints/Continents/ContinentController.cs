using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using World.Service.Database.Continents;

namespace World.Service.Endpoints.Continents;

[Route("api/continents")]
[ApiController]
public class ContinentController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ContinentOptions _options;
    private readonly IContinentRepo _repo;

    public ContinentController(IMapper mapper, IOptions<ContinentOptions> options, IContinentRepo repo)
    {
        _mapper = mapper;
        _options = options.Value;
        _repo = repo;
    }

    [HttpGet("all")]
    public async Task<ActionResult<ContinentsResponse>> GetAllContinents()
    {
        IEnumerable<ContinentEntity> allContinentEntities = await _repo.GetAllContinents();
        ContinentDto[] allContinentDtos = _mapper.Map<ContinentDto[]>(allContinentEntities);

        ContinentsResponse response = new();
        response.Continents = allContinentDtos;

        return Ok(response);
    }

    [HttpGet("top")]
    public async Task<ActionResult<ContinentsResponse>> GetTopContinents()
    {
        IEnumerable<ContinentEntity> topContinentEntities = await _repo.GetTopContinents(_options.TopCount);
        ContinentDto[] topContinentDtos = _mapper.Map<ContinentDto[]>(topContinentEntities);

        ContinentsResponse response = new();
        response.Continents = topContinentDtos;

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ContinentDto>> AddContinent([FromBody] AddContinentRequest request)
    {
        ContinentEntity continentEntity = await _repo.AddContinent(request.Name, request.Popularity);
        ContinentDto continentDto = _mapper.Map<ContinentDto>(continentEntity);

        return Ok(continentDto);
    }
}
