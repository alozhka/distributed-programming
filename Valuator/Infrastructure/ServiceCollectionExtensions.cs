using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static async Task AddServices(this IServiceCollection services)
    {
        services.AddScoped<TextRepository>();
        services.AddScoped<ValuatorService>();
        services.AddScoped<EventPublisher>();

        services.AddRedis();
        await services.AddRabbitMq();

        services.AddHostedService<RankEventListener>();
    }

    private static async Task AddRabbitMq(this IServiceCollection services)
    {
        ConnectionFactory rabbitmqConnectionFactory = new ConnectionFactory
        {
            HostName = "rabbitmq",
        };

        IConnection rabbitmqConnection = await rabbitmqConnectionFactory.CreateConnectionAsync();
        IChannel rabbitmqChannel = await rabbitmqConnection.CreateChannelAsync();
        await DeclareTypology(rabbitmqChannel);

        services.AddSingleton(rabbitmqChannel);
    }

    private static async Task DeclareTypology(IChannel rabbitmqChannel)
    {
        await rabbitmqChannel.ExchangeDeclareAsync(
            EventPublisher.RankExchangeName,
            ExchangeType.Direct
        );
        await rabbitmqChannel.QueueDeclareAsync(
            EventPublisher.RankQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await rabbitmqChannel.QueueBindAsync(
            EventPublisher.RankQueueName,
            EventPublisher.RankExchangeName,
            routingKey: ""
        );

        await rabbitmqChannel.ExchangeDeclareAsync(
            EventPublisher.SimilarityExchangeName,
            ExchangeType.Fanout
        );
    }

    private static void AddRedis(this IServiceCollection services)
    {
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis:6379");
        services.AddSingleton<IConnectionMultiplexer>(redis);

        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
            .SetApplicationName("Valuator");
    }
}