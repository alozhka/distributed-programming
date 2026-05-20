using Microsoft.Extensions.Hosting;

namespace ProtoKey.Storage;

public class PersistenceService(StorageService storageService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(period: TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await storageService.Flush(stoppingToken);
        }
    }
}