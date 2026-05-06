using StackExchange.Redis;

namespace RankCalculator.Shards;

public class RedisShardProvider(
    IConnectionMultiplexer main,
    IConnectionMultiplexer mainRu,
    IConnectionMultiplexer mainEu,
    IConnectionMultiplexer mainAsia
) : IShardProvider
{
    public IDatabase GetMain()
    {
        return main.GetDatabase();
    }

    public IDatabase GetShard(Region region)
    {
        return region switch
        {
            Region.Ru => mainRu.GetDatabase(),
            Region.Eu => mainEu.GetDatabase(),
            Region.Asia => mainAsia.GetDatabase(),
            _ => throw new ArgumentException($"Unsupported region {region} for sharding"),
        };
    }
}