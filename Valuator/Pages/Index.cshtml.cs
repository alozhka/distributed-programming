using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _db = redis.GetDatabase();

    public void OnGet()
    {
    }

    public IActionResult OnPost(string text)
    {
        logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        _db.StringSet(textKey, text);

        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        _db.StringSet(rankKey, rank.ToString(CultureInfo.InvariantCulture));

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text);
        _db.StringSet(similarityKey, similarity.ToString(CultureInfo.InvariantCulture));

        return Redirect($"summary?id={id}");
    }

    private static double CalculateRank(string text)
    {
        int nonAlphaCount = text.Count(c => !char.IsLetter(c));
        return (double)nonAlphaCount / text.Length;
    }

    private double CalculateSimilarity(string text)
    {
        // Ищем дубликат среди всех ранее сохранённых текстов.
        // Текущий текст уже сохранён, поэтому он сам с собой совпадёт (matchCount >= 1).
        // Если matchCount > 1, значит есть дубликат.
        IServer server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints()[0]);
        int matchCount = 0;
        foreach (var key in server.Keys(pattern: "TEXT-*"))
        {
            string? existingText = _db.StringGet(key);
            if (existingText == text)
            {
                matchCount++;
                if (matchCount > 1)
                {
                    return 1;
                }
            }
        }

        return 0;
    }
}