using Crypto.TradeEngine.Scene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.Testing.Infra.Mocks;

public class TestPlay : Play
{
    private TimeSpan _delayBeforeStop;

    public TestPlay(ILogger<TestPlay> logger, IOptions<ConsoleOptions> consoleOptions, IEnumerable<IActor> actors)
        : base(logger, consoleOptions, actors)
    { }

    public void SetDelayBeforeStop(TimeSpan delayBeforeStop)
    {
        _delayBeforeStop = delayBeforeStop;
    }

    protected override void WaitForStop()
    {
        Thread.Sleep(_delayBeforeStop);
    }
}
