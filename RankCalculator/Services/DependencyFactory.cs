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
        await DeclareTopology(channel);

        return channel;
    }

    public static RankCalculatorService CreateRankCalculatorService()
    {
        return new RankCalculatorService(CreateTextRepository());
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
            true,
            false,
            false
        );
    }
}