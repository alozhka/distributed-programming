using Microsoft.Extensions.Hosting;
using ProtoKey.Storage;

namespace ProtoKey.Workers;

public class StorageWorker(StorageService storageService) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return storageService.ProcessCommands(stoppingToken);
    }
}