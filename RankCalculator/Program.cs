using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RankCalculator.Services;

namespace RankCalculator;

class Program
{
    public static async Task Main()
    {
        IChannel channel = await DependencyFactory.CreateRabbitMqChannel();
        RankCalculatorService rankCalculator = DependencyFactory.CreateRankCalculatorService(channel);
        string consumerTag = await RunConsumer(channel, rankCalculator);

        await WaitToShutdown();

        await channel.BasicCancelAsync(consumerTag);
        Console.WriteLine("Shutdown completed");
    }

    private static async Task<string> RunConsumer(IChannel channel, RankCalculatorService rankCalculator)
    {
        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += (_, eventArgs) => ValuatorConsumer.Consume(channel, eventArgs, rankCalculator);

        return await channel.BasicConsumeAsync(
            ValuatorConsumer.QueueName,
            autoAck: false,
            consumer: consumer
        );
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