using Common.AspNet.Swagger;
using HandGame.Api.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HandGame.Api.Endpoints;

[ApiController]
public class GameController : ControllerBase
{
    private readonly IGame _game;
    private readonly ChoiceDtoFlyweight _choiceDtoFlyweight;

    public GameController(IGame game, ChoiceDtoFlyweight choiceDtoFlyweight)
    {
        _game = game;
        _choiceDtoFlyweight = choiceDtoFlyweight;
    }

    [HttpGet("/choices")]
    [OutputCache(Duration = int.MaxValue)]
    [ProducesResponseType(typeof(ChoicesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetChoices()
    {
        ChoicesResponse response = new()
        {
            Choices = _game
                .GetAllChoices()
                .Select(choice => _choiceDtoFlyweight.GetChoiceDto(choice.Id))
                .ToArray()
        };

        return Ok(response);
    }

    [HttpGet("/choice")]
    [ProducesResponseType(typeof(RandomChoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetRandomChoice()
    {
        Choice randomChoice = await _game.GetRandomChoice();

        RandomChoiceResponse response = new()
        {
            Choice = _choiceDtoFlyweight.GetChoiceDto(randomChoice.Id)
        };

        return Ok(response);
    }

    [HttpPost("/play")]
    [ProducesResponseType(typeof(PlayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Play([FromBody] PlayRequest request)
    {
        PlayResult playResult = await _game.Play(request.Player);
        PlayResponse response = new()
        {
            Player = request.Player,
            Bot = playResult.BotChoice,
            Result = playResult.Result
        };

        return Ok(response);
    }
}
