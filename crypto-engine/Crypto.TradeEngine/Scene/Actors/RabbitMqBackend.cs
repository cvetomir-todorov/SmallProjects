using System.Threading.Channels;
using Crypto.Contracts;
using Crypto.Threading;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Crypto.TradeEngine.Scene.Actors;

public sealed class RabbitMqBackend : IActor
{
    private readonly ILogger _logger;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ChannelReader<TradeInfo> _tradeReader;
    private readonly ChannelReader<TradeInfo> _volumeSpikeReader;
    private readonly ConnectionFactory _rabbitMqConnectionFactory;
    private IConnection? _rabbitMqConnection;
    private IModel? _tradeChannel;
    private IModel? _volumeSpikesChannel;
    private DedicatedThreadTaskScheduler? _tradesTaskScheduler;
    private DedicatedThreadTaskScheduler? _volumeSpikesTaskScheduler;
    private bool _disposed;

    public RabbitMqBackend(ILogger<RabbitMqBackend> logger, IOptions<RabbitMqOptions> rabbitMqOptions, ChannelContainer channelContainer)
    {
        _logger = logger;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _tradeReader = channelContainer.TradeBackend.Reader;
        _volumeSpikeReader = channelContainer.VolumeSpikeBackend.Reader;
        _rabbitMqConnectionFactory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.Hostname,
            Port = _rabbitMqOptions.Port,
            VirtualHost = _rabbitMqOptions.VirtualHost,
            RequestedConnectionTimeout = _rabbitMqOptions.ConnectTimeout,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _volumeSpikesTaskScheduler?.Dispose();
            _tradesTaskScheduler?.Dispose();
            _volumeSpikesChannel?.Dispose();
            _tradeChannel?.Dispose();
            _rabbitMqConnection?.Dispose();
            _disposed = true;
        }
    }

    public Task Start(CancellationToken ct)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port} vhost {VHost}...",
            _rabbitMqOptions.Hostname, _rabbitMqOptions.Port, _rabbitMqOptions.VirtualHost);
        _rabbitMqConnection = _rabbitMqConnectionFactory.CreateConnection();
        _tradeChannel = _rabbitMqConnection.CreateModel();
        _volumeSpikesChannel = _rabbitMqConnection.CreateModel();
        _logger.LogInformation("Successfully connected to RabbitMQ");

        // if RabbitMQ resources exists and are the same there will be no error or duplication
        // 'news-direct' exchange has:
        // - 'trades'        routing key -> 'trades'        queue
        // - 'volume-spikes' routing key -> 'volume-spikes' queue
        _tradeChannel.ExchangeDeclare("news-direct", ExchangeType.Direct, durable: true, autoDelete: false);
        _tradeChannel.QueueDeclare(queue: "trades", durable: true, exclusive: false, autoDelete: false);
        _tradeChannel.QueueDeclare(queue: "volume-spikes", durable: true, exclusive: false, autoDelete: false);
        _tradeChannel.QueueBind(queue: "trades", exchange: "news-direct", routingKey: "trades");
        _tradeChannel.QueueBind(queue: "volume-spikes", exchange: "news-direct", routingKey: "volume-spikes");

        _tradesTaskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(async () => await WriteTrades(ct), ct, TaskCreationOptions.None, _tradesTaskScheduler);

        _volumeSpikesTaskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(async () => await WriteVolumeSpikes(ct), ct, TaskCreationOptions.None, _volumeSpikesTaskScheduler);

        return Task.CompletedTask;
    }

    private async Task WriteTrades(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TradeInfo trade = await _tradeReader.ReadAsync(ct);
                TradeMessage tradeMessage = MapToTradeMessage(trade);
                // TODO: consider batch writing
                _tradeChannel.BasicPublish(exchange: "news-direct", routingKey: "trades", body: tradeMessage.ToByteArray(), mandatory: true);
            }
        }
        catch (OperationCanceledException)
        {
            // this means the cancellation token source has issued cancellation
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Error while writing trades to RabbitMQ");
        }
    }

    private async Task WriteVolumeSpikes(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TradeInfo trade = await _volumeSpikeReader.ReadAsync(ct);
                TradeMessage tradeMessage = MapToTradeMessage(trade);
                _volumeSpikesChannel.BasicPublish(exchange: "news-direct", routingKey: "volume-spikes", body: tradeMessage.ToByteArray(), mandatory: true);
            }
        }
        catch (OperationCanceledException)
        {
            // this means the cancellation token source has issued cancellation
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Error while writing volume spikes to RabbitMQ");
        }
    }

    private static TradeMessage MapToTradeMessage(TradeInfo trade)
    {
        return new TradeMessage
        {
            TradeTime = trade.TradeTime.ToTimestamp(),
            Symbol = trade.Symbol,
            Price = Convert.ToDouble(trade.Price),
            Quantity = Convert.ToDouble(trade.Quantity),
            Volume = Convert.ToDouble(trade.Volume)
        };
    }
}
