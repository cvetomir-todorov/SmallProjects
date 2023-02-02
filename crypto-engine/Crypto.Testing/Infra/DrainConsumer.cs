using Crypto.TradeEngine.Scene.Actors;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Crypto.Testing.Infra;

public class DrainConsumer
{
    private readonly ConnectionFactory _connectionFactory;
    private int _count;

    public DrainConsumer(RabbitMqOptions rabbitMqOptions)
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

    public int DrainTrades(TimeSpan interval)
    {
        return Drain("trades", interval);
    }

    public int DrainVolumeSpikes(TimeSpan interval)
    {
        return Drain("volume-spikes", interval);
    }

    private int Drain(string queue, TimeSpan interval)
    {
        using IConnection connection = _connectionFactory.CreateConnection();
        using IModel channel = connection.CreateModel();
        EventingBasicConsumer consumer = new(channel);

        try
        {
            _count = 0;
            consumer.Received += HandleMessage;
            channel.BasicConsume(queue, autoAck: true, consumer);
            Thread.Sleep(interval);

            return _count;
        }
        finally
        {
            consumer.Received -= HandleMessage;
        }
    }

    private void HandleMessage(object? sender, BasicDeliverEventArgs e)
    {
        Interlocked.Increment(ref _count);
    }
}
