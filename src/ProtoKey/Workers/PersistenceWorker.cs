using Microsoft.Extensions.Hosting;
using ProtoKey.Storage;

namespace ProtoKey.Workers;

public class PersistenceWorker(PersistenceService persistenceService) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await persistenceService.Load(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(period: TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await persistenceService.Flush(stoppingToken);
        }
    }
}