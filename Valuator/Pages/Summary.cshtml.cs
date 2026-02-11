using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel(ILogger<SummaryModel> logger, ValuatorService valuatorService) : PageModel
{
    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        logger.LogDebug(id);

        Rank = valuatorService.GetRank(id);
        Similarity = valuatorService.GetSimilarity(id);
    }
}