using System.Text.Json;
using EventsLogger.Events;
using RabbitMQ.Client.Events;

namespace EventsLogger.Services;

public class EventsConsumer
{
    public const string RankCalculatedExchange = "rank_calculator.events.rank_calculated";
    public const string SimilarityCalculatedExchange = "valuator.events.similarity_calculated";

    public static Task Consume(BasicDeliverEventArgs eventArgs) => eventArgs.Exchange switch
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