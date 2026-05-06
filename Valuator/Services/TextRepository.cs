using System.Globalization;
using StackExchange.Redis;
using Valuator.Shards;

namespace Valuator.Services;

public class TextRepository(
    IShardProvider shardProvider,
    ILogger<TextRepository> logger
)
{
    private const string ShardKeyPrefix = "SHARD-";
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private const string SimilarityKeyPrefix = "SIMILARITY-";

    private readonly IDatabase _main = shardProvider.GetMain();

    public void SaveText(string id, string text, Region region)
    {
        _main.StringSet(ShardKeyPrefix + id, region.ToString());
        shardProvider.GetShard(region).StringSet(TextKeyPrefix + id, text);
    }

    public void SaveSimilarity(string id, double similarity)
    {
        Region region = GetRegionById(id);
        IDatabase shard = shardProvider.GetShard(region);
        shard.StringSet(SimilarityKeyPrefix + id, similarity.ToString(CultureInfo.InvariantCulture));
    }

    public IEnumerable<string> ListTexts(Region region)
    {
        IDatabase shard = shardProvider.GetShard(region);
        IServer server = shard.Multiplexer.GetServer(shard.Multiplexer.GetEndPoints()[0]);
        foreach (var key in server.Keys(pattern: $"{TextKeyPrefix}*"))
        {
            string? text = shard.StringGet(key);
            if (text != null)
            {
                yield return text;
            }
        }
    }

    public double? GetRank(string id)
    {
        Region region = GetRegionById(id);
        string? value = shardProvider.GetShard(region).StringGet(RankKeyPrefix + id);
        return value == null ? null : double.Parse(value, CultureInfo.InvariantCulture);
    }

    public double? GetSimilarity(string id)
    {
        Region region = GetRegionById(id);
        string? value = shardProvider.GetShard(region).StringGet(SimilarityKeyPrefix + id);
        return value == null ? null : double.Parse(value, CultureInfo.InvariantCulture);
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

        logger.LogInformation("LOOKUP: {id}, {region}", id, region);

        return region;
    }
}