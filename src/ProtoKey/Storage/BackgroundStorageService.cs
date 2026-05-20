using Microsoft.Extensions.Hosting;

namespace ProtoKey.Storage;

public class BackgroundStorageService(StorageService storage) : BackgroundService
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await storage.Load(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return storage.ProcessCommands(stoppingToken);
    }
}