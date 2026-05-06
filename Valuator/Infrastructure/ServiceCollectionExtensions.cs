using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Services;
using Valuator.Shards;

namespace Valuator.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static async Task AddServices(this IServiceCollection services)
    {
        services.AddScoped<TextRepository>();
        services.AddScoped<ValuatorService>();
        services.AddScoped<EventPublisher>();
        services.AddScoped<IShardProvider, RedisShardProvider>();

        services.AddRedis();
        await services.AddRabbitMq();
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
        ConnectionMultiplexer mainRedis = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable("DB_MAIN")!
        );
        services.AddSingleton<IConnectionMultiplexer>(mainRedis);

        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(mainRedis, "DataProtection-Keys")
            .SetApplicationName("Valuator");

        ConnectionMultiplexer mainRu = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable("DB_RU")!
        );
        services.AddKeyedSingleton<IConnectionMultiplexer>(Region.Ru, mainRu);

        ConnectionMultiplexer mainEu = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable("DB_EU")!
        );
        services.AddKeyedSingleton<IConnectionMultiplexer>(Region.Eu, mainEu);

        ConnectionMultiplexer mainAsia = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable("DB_ASIA")!
        );
        services.AddKeyedSingleton<IConnectionMultiplexer>(Region.Asia, mainAsia);
    }
}