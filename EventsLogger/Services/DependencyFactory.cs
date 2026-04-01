using RabbitMQ.Client;

namespace EventsLogger.Services;

public class DependencyFactory
{
    public static async Task<IChannel> CreateRabbitMqChannel()
    {
        ConnectionFactory factory = new ConnectionFactory { HostName = "rabbitmq", };
        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();
        await DeclareTopology(channel);
        return channel;
    }

    private static async Task DeclareTopology(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(EventsConsumer.RankCalculatedExchange, ExchangeType.Fanout);
        await channel.ExchangeDeclareAsync(EventsConsumer.SimilarityCalculatedExchange, ExchangeType.Fanout);
    }

    public static async Task<(string RankQueueName, string SimilarityQueueName)> CreateQueues(IChannel channel)
    {
        QueueDeclareOk rankQueue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true
        );
        await channel.QueueBindAsync(
            rankQueue.QueueName,
            EventsConsumer.RankCalculatedExchange,
            routingKey: ""
        );

        QueueDeclareOk similarityQueue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true
        );
        await channel.QueueBindAsync(
            similarityQueue.QueueName,
            EventsConsumer.SimilarityCalculatedExchange,
            routingKey: ""
        );

        return (rankQueue.QueueName, similarityQueue.QueueName);
    }
}