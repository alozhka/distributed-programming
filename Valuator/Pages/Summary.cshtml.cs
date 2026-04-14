using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel(
    ValuatorService valuatorService,
    ILogger<SummaryModel> logger
) : PageModel
{
    public string Id { get; private set; } = string.Empty;
    public double? Rank { get; private set; }
    public double Similarity { get; private set; }

    public IActionResult OnGet(string id)
    {
        Id = id;

        if (string.IsNullOrEmpty(Id))
        {
            return RedirectToPage("Index");
        }

        try
        {
            Rank = valuatorService.GetRank(Id);
            Similarity = valuatorService.GetSimilarity(Id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error: {message}\n{trace}", ex.Message, ex.StackTrace);
            return RedirectToPage("Index");
        }

        return Page();
    }
}