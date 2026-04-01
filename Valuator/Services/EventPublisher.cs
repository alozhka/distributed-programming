using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Valuator.Events;

namespace Valuator.Services;

public class EventPublisher(IChannel rabbitmqChannel)
{
    public const string RankExchangeName = "valuator.processing.rank";
    public const string RankQueueName = "valuator.processing.rank";
    public const string SimilarityExchangeName = "valuator.events.similarity_calculated";

    public async Task PublishRank(string id)
    {
        byte[] body = Encoding.UTF8.GetBytes(id);
        await rabbitmqChannel.BasicPublishAsync(
            exchange: RankExchangeName,
            routingKey: "",
            mandatory: false,
            body: body
        );
    }

    public async Task NotifySimilarityCalculated(string id, double similarity)
    {
        SimilarityCalculatedEvent @event = new(id, similarity);
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        await rabbitmqChannel.BasicPublishAsync(
            exchange: SimilarityExchangeName,
            routingKey: "",
            mandatory: false,
            body: body
        );
    }
}