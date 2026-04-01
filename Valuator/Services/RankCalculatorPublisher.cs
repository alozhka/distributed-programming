using System.Text;
using RabbitMQ.Client;

namespace Valuator.Services;

public class RankCalculatorPublisher(IChannel rabbitmqChannel)
{
    public const string ExchangeName = "valuator.processing.rank";
    public const string QueueName = "valuator.processing.rank";

    public async Task PublishRank(string id)
    {
        byte[] body = Encoding.UTF8.GetBytes(id);
        await rabbitmqChannel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "",
            mandatory: false,
            body: body
        );
    }
}