using System.Text;
using System.Text.Json;
using EventsLogger.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string RankCalculatedExchange = "valuator.events.rank_calculated";
const string SimilarityCalculatedExchange = "valuator.events.similarity_calculated";

IConnectionFactory factory = new ConnectionFactory { HostName = "rabbitmq" };
IConnection connection = await factory.CreateConnectionAsync();
IChannel channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(RankCalculatedExchange, ExchangeType.Fanout);
await channel.ExchangeDeclareAsync(SimilarityCalculatedExchange, ExchangeType.Fanout);

QueueDeclareOk rankQueue = await channel.QueueDeclareAsync(
    queue: "",
    durable: false,
    exclusive: true,
    autoDelete: true
);
await channel.QueueBindAsync(rankQueue.QueueName, RankCalculatedExchange, routingKey: "");

QueueDeclareOk similarityQueue = await channel.QueueDeclareAsync(
    queue: "",
    durable: false,
    exclusive: true,
    autoDelete: true
);
await channel.QueueBindAsync(similarityQueue.QueueName, SimilarityCalculatedExchange, routingKey: "");

AsyncEventingBasicConsumer consumer = new(channel);
consumer.ReceivedAsync += (_, eventArgs) =>
{
    string body = Encoding.UTF8.GetString(eventArgs.Body.Span);

    if (eventArgs.Exchange == RankCalculatedExchange)
    {
        var e = JsonSerializer.Deserialize<RankCalculatedEvent>(body)!;
        Console.WriteLine($"[RankCalculated] Id={e.Id} Rank={e.Rank}");
    }
    else if (eventArgs.Exchange == SimilarityCalculatedExchange)
    {
        var e = JsonSerializer.Deserialize<SimilarityCalculatedEvent>(body)!;
        Console.WriteLine($"[SimilarityCalculated] Id={e.Id} Similarity={e.Similarity}");
    }

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(rankQueue.QueueName, autoAck: true, consumer: consumer);
await channel.BasicConsumeAsync(similarityQueue.QueueName, autoAck: true, consumer: consumer);

Console.WriteLine("EventsLogger started. Waiting for events...");

TaskCompletionSource tsc = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; tsc.SetResult(); };
AppDomain.CurrentDomain.ProcessExit += (_, _) => tsc.SetResult();
await tsc.Task;