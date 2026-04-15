using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Valuator.Events;
using Valuator.Hubs;

namespace Valuator.Services;

public class RankEventListener(IChannel channel, IHubContext<RankHub> rankHubConext) : BackgroundService
{
    private const string RankCalculatedExchange = "rank_calculator.events.rank_calculated";
    private string _queueName = string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SubscribeToQueue(stoppingToken);
        await RunConsume(stoppingToken);
    }

    private async Task SubscribeToQueue(CancellationToken ct = default)
    {
        _queueName = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: ct
        );
        await channel.QueueBindAsync(
            queue: _queueName,
            exchange: RankCalculatedExchange,
            routingKey: "",
            cancellationToken: ct
        );
    }

    private async Task RunConsume(CancellationToken stoppingToken)
    {
        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += (_, args) => args.Exchange switch
        {
            RankCalculatedExchange => HandleRankCalculated(args, stoppingToken),
        };

        await channel.BasicConsumeAsync(_queueName, autoAck: true, consumer, stoppingToken);
    }

    private Task HandleRankCalculated(BasicDeliverEventArgs eventArgs, CancellationToken ct = default)
    {
        string json = Encoding.UTF8.GetString(eventArgs.Body.Span);
        RankCalculatedEvent rankEvent = JsonSerializer.Deserialize<RankCalculatedEvent>(json)!;
        Console.WriteLine($"[RankCalculated] Id={rankEvent.Id} Rank={rankEvent.Rank}");

        return rankHubConext.Clients.Group(rankEvent.Id).SendAsync(
            "RankCalculated",
            rankEvent.Rank,
            cancellationToken: ct
        );
    }
}