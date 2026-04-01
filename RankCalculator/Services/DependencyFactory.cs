using RabbitMQ.Client;
using StackExchange.Redis;

namespace RankCalculator.Services;

public class DependencyFactory
{
    public static async Task<IChannel> CreateRabbitMqChannel()
    {
        IConnectionFactory factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
        };
        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
        await DeclareTopology(channel);

        return channel;
    }

    public static RankCalculatorService CreateRankCalculatorService(IChannel channel)
    {
        return new RankCalculatorService(CreateTextRepository(), CreateEventPublisher(channel));
    }

    public static EventPublisher CreateEventPublisher(IChannel channel)
    {
        return new EventPublisher(channel);
    }

    public static TextRepository CreateTextRepository()
    {
        return new TextRepository(CreateRedisConnection());
    }

    public static IConnectionMultiplexer CreateRedisConnection()
    {
        return ConnectionMultiplexer.Connect("redis:6379");
    }

    private static async Task DeclareTopology(IChannel channel)
    {
        await channel.QueueDeclareAsync(
            ValuatorConsumer.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await channel.ExchangeDeclareAsync(
            EventPublisher.RankCalculatedExchangeName,
            ExchangeType.Fanout
        );
    }
}