using Microsoft.Extensions.DependencyInjection;
using ProtoKey.Storage;

namespace ProtoKey;

public static class ServiceCollectionExtensions
{
    public static void AddProtoKey(this IServiceCollection s)
    {
        s.AddSingleton<StorageService>();
        s.AddHostedService<BackgroundStorageService>();
    }
}