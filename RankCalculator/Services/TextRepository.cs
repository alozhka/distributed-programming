using System.Globalization;
using RankCalculator.Shards;
using StackExchange.Redis;

namespace RankCalculator.Services;

public class TextRepository(IShardProvider shardProvider)
{
    private const string ShardKeyPrefix = "SHARD-";
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private readonly IDatabase _main = shardProvider.GetMain();

    public string? GetText(string id)
    {
        Region region = GetRegionById(id);
        IDatabase shard = shardProvider.GetShard(region);

        return shard.StringGet(TextKeyPrefix + id);
    }

    public void SaveRank(string id, double rank)
    {
        Region region = GetRegionById(id);
        IDatabase shard = shardProvider.GetShard(region);

        shard.StringSet(RankKeyPrefix + id, rank.ToString(CultureInfo.InvariantCulture));
    }

    private Region GetRegionById(string id)
    {
        string? regionStr = _main.StringGet(ShardKeyPrefix + id);

        if (string.IsNullOrEmpty(regionStr))
        {
            throw new KeyNotFoundException($"Shard was not found for id {id}");
        }

        if (!Enum.TryParse(regionStr, out Region region))
        {
            throw new InvalidOperationException($"Region {regionStr} is not supported");
        }

        Console.WriteLine("LOOKUP: {0}, {1}", id, region);

        return region;
    }
}