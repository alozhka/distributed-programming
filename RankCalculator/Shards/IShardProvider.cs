using StackExchange.Redis;

namespace RankCalculator.Shards;

public interface IShardProvider
{
    IDatabase GetMain();
    IDatabase GetShard(Region region);
}