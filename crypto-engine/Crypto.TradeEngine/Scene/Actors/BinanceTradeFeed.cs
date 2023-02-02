using System.Threading.Channels;
using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.TradeEngine.Scene.Actors;

public interface ITradeFeed : IActor
{ }

public sealed class BinanceTradeFeed : ITradeFeed
{
    private readonly ILogger _logger;
    private readonly ConsoleOptions _consoleOptions;
    private readonly BinanceSocketClient _binanceClient;
    private readonly ChannelWriter<TradeInfo> _localFileWriter;
    private readonly ChannelWriter<TradeInfo> _volumeSpikeWriter;
    private readonly ChannelWriter<TradeInfo> _backendWriter;
    private readonly Dictionary<string, SymbolOptions> _symbols;
    private bool _disposed;
    private ulong _tradeCount;

    public BinanceTradeFeed(
        ILogger<BinanceTradeFeed> logger,
        IOptions<ConsoleOptions> consoleOptions,
        IOptions<BinanceOptions> binanceOptions,
        IOptions<SymbolConfigOptions> symbolConfigOptions,
        ChannelContainer channelContainer)
    {
        _logger = logger;
        _consoleOptions = consoleOptions.Value;
        _binanceClient = new BinanceSocketClient(new BinanceSocketClientOptions
        {
            ApiCredentials = new(binanceOptions.Value.ApiKey, binanceOptions.Value.ApiSecret)
        });
        _localFileWriter = channelContainer.LocalFile.Writer;
        _volumeSpikeWriter = channelContainer.VolumeSpike.Writer;
        _backendWriter = channelContainer.TradeBackend.Writer;
        _symbols = new Dictionary<string, SymbolOptions>();

        foreach (SymbolOptions symbol in symbolConfigOptions.Value.Symbols)
        {
            _symbols.Add(symbol.Symbol, symbol);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _binanceClient.Dispose();
            _disposed = true;
        }
    }

    public async Task Start(CancellationToken ct)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        IReadOnlyCollection<string> symbols = _symbols.Keys;
        _logger.LogInformation("Subscribing to Binance for trades about {SymbolCount} symbols [{Symbols}]...", symbols.Count, string.Join(", ", symbols));
        // TODO: figure out how to subscribe to Binance when some trades have been lost
        CallResult<UpdateSubscription> subscription = await _binanceClient.SpotStreams.SubscribeToTradeUpdatesAsync(
            symbols, dataEvent => OnMessage(dataEvent, ct), ct);

        if (subscription.Success)
        {
            _logger.LogInformation("Successfully subscribed to Binance");
        }
        else
        {
            _logger.LogInformation("Failed to subscribe to Binance: {Error}", subscription.Error);
        }
    }

    private void OnMessage(DataEvent<BinanceStreamTrade> e, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }
        if (!_symbols.TryGetValue(e.Data.Symbol, out SymbolOptions? symbol))
        {
            _logger.LogError("Missing config for symbol {Symbol}", e.Data.Symbol);
            return;
        }

        TradeInfo trade = new()
        {
            TradeTime = e.Data.TradeTime,
            Symbol = e.Data.Symbol,
            Price = e.Data.Price,
            Quantity = e.Data.Quantity,
            Volume = e.Data.Price * e.Data.Quantity,
            SymbolConfig = symbol
        };

        ProcessTrade(trade);
    }

    private void ProcessTrade(TradeInfo trade)
    {
        IncreaseCount();

        if (_consoleOptions.OutputEnabled)
        {
            _logger.LogInformation("{Trade}", trade);
        }
        if (!_localFileWriter.TryWrite(trade))
        {
            _logger.LogError("Failed to send trade to local file");
        }
        if (!_volumeSpikeWriter.TryWrite(trade))
        {
            _logger.LogError("Failed to send trade to volume spike");
        }
        if (!_backendWriter.TryWrite(trade))
        {
            _logger.LogError("Failed to send trade to backend");
        }
    }

    // TODO: replace with metrics
    private void IncreaseCount()
    {
        // this is not called from 2 threads at the same time according to binance
        _tradeCount++;
        if (_tradeCount % 1000 == 0)
        {
            _logger.LogWarning("{TradeCount} trades from Binance received", _tradeCount);
            if (_tradeCount == ulong.MinValue)
            {
                _logger.LogError("Trade counter from Binance was reset to 0");
            }
        }
    }
}
