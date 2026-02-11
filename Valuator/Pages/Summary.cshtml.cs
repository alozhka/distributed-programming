using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel(
    ValuatorService valuatorService,
    ILogger<SummaryModel> logger
) : PageModel
{
    public double Rank { get; set; }
    public double Similarity { get; set; }

    public IActionResult OnGet(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("Index");
        }

        try
        {
            Rank = valuatorService.GetRank(id);
            Similarity = valuatorService.GetSimilarity(id);
        }
        catch (Exception ex)
        {
            logger.LogError("Error: {message}\n{trace}", ex.Message, ex.StackTrace);
            return RedirectToPage("Index");
        }

        return Page();
    }
}