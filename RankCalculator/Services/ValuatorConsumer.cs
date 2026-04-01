using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RankCalculator.Services;

public class ValuatorConsumer
{
    public const string QueueName = "valuator.processing.rank";

    public static async Task Consume(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        RankCalculatorService rankCalculator
    )
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.Span);
        Console.WriteLine($"Consuming message: {message} from {eventArgs.Exchange}");

        await rankCalculator.CalculateRank(message);

        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }
}