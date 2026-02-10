using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _db = redis.GetDatabase();

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        logger.LogDebug(id);

        string rankKey = "RANK-" + id;
        string? rankValue = _db.StringGet(rankKey);
        Rank = rankValue != null ? double.Parse(rankValue, System.Globalization.CultureInfo.InvariantCulture) : 0;

        string similarityKey = "SIMILARITY-" + id;
        string? similarityValue = _db.StringGet(similarityKey);
        Similarity = similarityValue != null ? double.Parse(similarityValue, System.Globalization.CultureInfo.InvariantCulture) : 0;
    }
}
