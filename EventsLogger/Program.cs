using EventsLogger.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger;

class Program
{
    public static async Task Main()
    {
        IChannel channel = await DependencyFactory.CreateRabbitMqChannel();
        (string rankQueue, string similarityQueue) = await DependencyFactory.CreateQueues(channel);
        await RunConsumer(channel, rankQueue, similarityQueue);

        Console.WriteLine("EventsLogger started. Waiting for events...");
        await WaitToShutdown();
    }

    private static async Task RunConsumer(IChannel channel, string rankQueue, string similarityQueue)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (_, eventArgs) => EventsConsumer.Consume(eventArgs);

        await channel.BasicConsumeAsync(rankQueue, autoAck: true, consumer: consumer);
        await channel.BasicConsumeAsync(similarityQueue, autoAck: true, consumer: consumer);
    }

    private static Task WaitToShutdown()
    {
        TaskCompletionSource tsc = new();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            tsc.SetResult();
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => tsc.SetResult();
        return tsc.Task;
    }
}