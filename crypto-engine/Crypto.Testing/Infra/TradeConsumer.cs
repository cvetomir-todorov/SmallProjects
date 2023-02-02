using Crypto.Contracts;
using Crypto.TradeEngine.Scene.Actors;
using Google.Protobuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Crypto.Testing.Infra;

public class TradeConsumer
{
    private readonly ConnectionFactory _connectionFactory;

    public TradeConsumer(RabbitMqOptions rabbitMqOptions)
    {
        _connectionFactory = new()
        {
            HostName = rabbitMqOptions.Hostname,
            Port = rabbitMqOptions.Port,
            VirtualHost = rabbitMqOptions.VirtualHost,
            RequestedConnectionTimeout = rabbitMqOptions.ConnectTimeout,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password
        };
    }

    public TradeConsumerContext StartCollectingTrades()
    {
        return Start("trades");
    }

    public TradeConsumerContext StartCollectingVolumeSpikes()
    {
        return Start("volume-spikes");
    }

    private TradeConsumerContext Start(string queue)
    {
        IConnection connection = _connectionFactory.CreateConnection();
        IModel channel = connection.CreateModel();
        TradeConsumerContext context = new (connection, channel);

        EventingBasicConsumer consumer = new(channel);
        consumer.Received += (_, e) =>
        {
            TradeMessage tradeMessage = new();
            tradeMessage.MergeFrom(e.Body.Span);
            context.TradeMessages.Add(tradeMessage);
        };

        channel.BasicConsume(queue, autoAck: true, consumer);
        return context;
    }
}

public sealed class TradeConsumerContext : IDisposable
{
    public TradeConsumerContext(IConnection connection, IModel channel)
    {
        Connection = connection;
        Channel = channel;
        TradeMessages = new List<TradeMessage>();
    }

    public IConnection Connection { get; }
    public IModel Channel { get; }
    public List<TradeMessage> TradeMessages { get; }

    public void Dispose()
    {
        Channel.Dispose();
        Connection.Dispose();
    }
}
