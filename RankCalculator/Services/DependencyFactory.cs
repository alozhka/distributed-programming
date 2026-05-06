using RabbitMQ.Client;
using RankCalculator.Shards;
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

    private static EventPublisher CreateEventPublisher(IChannel channel)
    {
        return new EventPublisher(channel);
    }

    private static TextRepository CreateTextRepository()
    {
        return new TextRepository(CreateShardsProvider());
    }

    private static RedisShardProvider CreateShardsProvider()
    {
        return new RedisShardProvider(
            CreateRedisConnection("DB_MAIN"),
            CreateRedisConnection("DB_RU"),
            CreateRedisConnection("DB_EU"),
            CreateRedisConnection("DB_ASIA")
        );
    }

    private static ConnectionMultiplexer CreateRedisConnection(string envName)
    {
        return ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(envName)!);
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