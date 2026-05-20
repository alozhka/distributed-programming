using Microsoft.Extensions.DependencyInjection;
using ProtoKey.Storage;
using ProtoKey.Workers;

namespace ProtoKey;

public static class ServiceCollectionExtensions
{
    public static void AddProtoKey(this IServiceCollection s)
    {
        s.AddSingleton<KeyValueStorage>();
        s.AddSingleton<StorageScheduler>();
        s.AddSingleton<StorageService>();
        s.AddSingleton<PersistenceService>();
        s.AddHostedService<StorageWorker>();
        s.AddHostedService<PersistenceWorker>();
    }
}