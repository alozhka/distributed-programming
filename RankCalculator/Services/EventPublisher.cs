using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RankCalculator.Events;

namespace RankCalculator.Services;

public class EventPublisher(IChannel channel)
{
    public const string RankCalculatedExchangeName = "rank_calculator.events.rank_calculated";

    public async Task NotifyRankCalculated(string id, double rank)
    {
        RankCalculated @event = new(id, rank);
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        await channel.BasicPublishAsync(
            exchange: RankCalculatedExchangeName,
            routingKey: "",
            mandatory: false,
            body: body
        );
    }
}