using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RankCalculator.Services;

public class ValuatorConsumer(IChannel channel, RankCalculatorService rankCalculator) : BackgroundService
{
    public const string QueueName = "valuator.processing.rank";

    private string? _consumerTag;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += OnReceived;

        _consumerTag = await channel.BasicConsumeAsync(
            QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumerTag is not null)
        {
            await channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }

    private async Task OnReceived(object _, BasicDeliverEventArgs eventArgs)
    {
        string message = Encoding.UTF8.GetString(eventArgs.Body.Span);
        Console.WriteLine($"Consuming message: {message} from {eventArgs.Exchange}");

        await rankCalculator.CalculateRank(message);

        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
    }
}
