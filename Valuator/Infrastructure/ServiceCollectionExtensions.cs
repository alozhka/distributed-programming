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
        services.AddScoped<RankCalculatorPublisher>();

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
        await rabbitmqChannel.ExchangeDeclareAsync(
            RankCalculatorPublisher.ExchangeName,
            ExchangeType.Direct
        );
        await rabbitmqChannel.QueueDeclareAsync(
            RankCalculatorPublisher.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await rabbitmqChannel.QueueBindAsync(
            RankCalculatorPublisher.QueueName,
            RankCalculatorPublisher.ExchangeName,
            routingKey: ""
        );

        services.AddSingleton(rabbitmqChannel);
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