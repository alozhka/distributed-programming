using StackExchange.Redis;

namespace Valuator.Shards;

public interface IShardProvider
{
    IDatabase GetMain();
    IDatabase GetShard(Region region);
}