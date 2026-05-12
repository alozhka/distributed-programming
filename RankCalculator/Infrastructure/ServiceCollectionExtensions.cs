using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RankCalculator.Services;
using StackExchange.Redis;

namespace RankCalculator.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static async Task AddServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton<TextRepository>();
        services.AddSingleton<RankCalculatorService>();
        services.AddSingleton<EventPublisher>();

        services.AddRedis(cfg);
        await services.AddRabbitMq(cfg);
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration cfg)
    {
        string redisConnectionString = cfg.GetConnectionString("Redis")!;
        IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton(redis);
    }

    private static async Task AddRabbitMq(this IServiceCollection services, IConfiguration cfg)
    {
        string rabbitConnectionString = cfg.GetConnectionString("RabbitMq")!;
        ConnectionFactory factory = new() { Uri = new Uri(rabbitConnectionString) };

        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
        await DeclareTopology(channel);

        services.AddSingleton(channel);
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
