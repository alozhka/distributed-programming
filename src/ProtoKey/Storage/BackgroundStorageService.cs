using Microsoft.Extensions.Hosting;

namespace ProtoKey.Storage;

public class BackgroundStorageService(StorageService storage) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return storage.Process(stoppingToken);
    }
}