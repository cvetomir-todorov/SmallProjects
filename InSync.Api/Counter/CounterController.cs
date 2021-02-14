using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InSync.Api.Counter
{
    public sealed class GetStatusResponse
    {
        public bool Status { get; set; }
    }

    public sealed class SetStatusRequest
    {
        public bool NewStatus { get; set; }
    }

    // we can describe the operations/endpoints using swagger
    [ApiController]
    [Route("api/counter-status")]
    public sealed class CounterController : ControllerBase
    {
        private readonly ICounterClient _counterClient;

        public CounterController(ICounterClient counterClient)
        {
            _counterClient = counterClient;
        }

        [HttpGet]
        public async Task<ActionResult> GetStatus()
        {
            GetStatusResult result = await _counterClient.GetStatus();
            if (!result.IsSuccess)
            {
                return new ObjectResult("Connection to counter failed.")
                {
                    StatusCode = StatusCodes.Status502BadGateway
                };
            }

            GetStatusResponse response = new GetStatusResponse
            {
                Status = result.Status
            };
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult> SetStatus([FromBody] SetStatusRequest request)
        {
            SetStatusResult result = await _counterClient.SetStatus(request.NewStatus);
            if (!result.IsSuccess)
            {
                return new ObjectResult("Connection to counter failed.")
                {
                    StatusCode = StatusCodes.Status502BadGateway
                };
            }

            return Ok();
        }
    }
}
