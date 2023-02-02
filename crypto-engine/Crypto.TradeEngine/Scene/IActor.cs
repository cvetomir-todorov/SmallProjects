namespace Crypto.TradeEngine.Scene;

public interface IActor : IDisposable
{
    Task Start(CancellationToken ct);
}
