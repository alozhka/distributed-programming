using EventsLogger.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EventsLogger.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static async Task AddServices(this IServiceCollection services, IConfiguration cfg)
    {
        string rabbitConnectionString = cfg.GetConnectionString("RabbitMq")!;
        ConnectionFactory factory = new() { Uri = new Uri(rabbitConnectionString) };

        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();
        await DeclareTopology(channel);

        services.AddSingleton(channel);
    }

    private static async Task DeclareTopology(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(EventsConsumer.RankCalculatedExchange, ExchangeType.Fanout);
        await channel.ExchangeDeclareAsync(EventsConsumer.SimilarityCalculatedExchange, ExchangeType.Fanout);
    }
}
