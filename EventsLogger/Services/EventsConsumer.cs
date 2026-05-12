using System.Text.Json;
using EventsLogger.Events;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger.Services;

public class EventsConsumer(IChannel channel) : BackgroundService
{
    public const string RankCalculatedExchange = "valuator.events.rank_calculated";
    public const string SimilarityCalculatedExchange = "valuator.events.similarity_calculated";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string rankQueue = await DeclareAndBind(RankCalculatedExchange, stoppingToken);
        string similarityQueue = await DeclareAndBind(SimilarityCalculatedExchange, stoppingToken);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (_, eventArgs) => Consume(eventArgs);

        await channel.BasicConsumeAsync(rankQueue, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
        await channel.BasicConsumeAsync(similarityQueue, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

        Console.WriteLine("EventsLogger started. Waiting for events...");
    }

    private async Task<string> DeclareAndBind(string exchange, CancellationToken cancellationToken)
    {
        QueueDeclareOk queue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: cancellationToken
        );
        await channel.QueueBindAsync(queue.QueueName, exchange, routingKey: "", cancellationToken: cancellationToken);
        return queue.QueueName;
    }

    private static Task Consume(BasicDeliverEventArgs eventArgs) => eventArgs.Exchange switch
    {
        RankCalculatedExchange => ConsumeRankCalculated(eventArgs),
        SimilarityCalculatedExchange => ConsumeSimilarityCalculated(eventArgs),
        _ => Task.CompletedTask,
    };

    private static Task ConsumeRankCalculated(BasicDeliverEventArgs eventArgs)
    {
        RankCalculatedEvent e = JsonSerializer.Deserialize<RankCalculatedEvent>(eventArgs.Body.Span)!;

        Console.WriteLine($"[RankCalculated] Id={e.Id} Rank={e.Rank}");

        return Task.CompletedTask;
    }

    private static Task ConsumeSimilarityCalculated(BasicDeliverEventArgs eventArgs)
    {
        SimilarityCalculatedEvent e = JsonSerializer.Deserialize<SimilarityCalculatedEvent>(eventArgs.Body.Span)!;

        Console.WriteLine($"[SimilarityCalculated] Id={e.Id} Similarity={e.Similarity}");

        return Task.CompletedTask;
    }
}
