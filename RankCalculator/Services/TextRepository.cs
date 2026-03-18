using System.Globalization;
using StackExchange.Redis;

namespace RankCalculator.Services;

public class TextRepository(IConnectionMultiplexer redis)
{
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private readonly IDatabase _db = redis.GetDatabase();

    public string? GetText(string id)
    {
        return _db.StringGet(TextKeyPrefix + id);
    }

    public void SaveRank(string id, double rank)
    {
        _db.StringSet(RankKeyPrefix + id, rank.ToString(CultureInfo.InvariantCulture));
    }
}