using System.Globalization;
using StackExchange.Redis;

namespace Valuator.Services;

public class TextRepository(IConnectionMultiplexer redis)
{
    private const string TextKeyPrefix = "TEXT-";
    private const string RankKeyPrefix = "RANK-";
    private const string SimilarityKeyPrefix = "SIMILARITY-";
    private const string AuthorKeyPrefix = "AUTHOR-";
    private readonly IDatabase _db = redis.GetDatabase();

    public void SaveText(string id, string text)
    {
        _db.StringSet(TextKeyPrefix + id, text);
    }

    public void SaveAuthor(string id, string author)
    {
        _db.StringSet(AuthorKeyPrefix + id, author);
    }

    public string? GetAuthor(string id)
    {
        return _db.StringGet(AuthorKeyPrefix + id);
    }

    public void SaveRank(string id, double rank)
    {
        _db.StringSet(RankKeyPrefix + id, rank.ToString(CultureInfo.InvariantCulture));
    }

    public void SaveSimilarity(string id, double similarity)
    {
        _db.StringSet(SimilarityKeyPrefix + id, similarity.ToString(CultureInfo.InvariantCulture));
    }

    public IEnumerable<string> ListTexts()
    {
        IServer server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        foreach (var key in server.Keys(pattern: $"{TextKeyPrefix}*"))
        {
            string? text = _db.StringGet(key);
            if (text != null)
            {
                yield return text;
            }
        }
    }

    public double? GetRank(string id)
    {
        string? value = _db.StringGet(RankKeyPrefix + id);
        return value == null ? null : double.Parse(value, CultureInfo.InvariantCulture);
    }

    public double? GetSimilarity(string id)
    {
        string? value = _db.StringGet(SimilarityKeyPrefix + id);
        return value == null ? null : double.Parse(value, CultureInfo.InvariantCulture);
    }
}