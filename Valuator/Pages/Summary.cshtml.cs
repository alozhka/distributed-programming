using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

[Authorize]
public class SummaryModel(
    ValuatorService valuatorService,
    ILogger<SummaryModel> logger
) : PageModel
{
    public double? Rank { get; set; }
    public double Similarity { get; set; }

    public IActionResult OnGet(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToPage("Index");
        }

        try
        {
            string username = User.Identity!.Name!;
            Rank = valuatorService.GetRank(id, username);
            Similarity = valuatorService.GetSimilarity(id, username);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Access denied: {message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            logger.LogError("Error: {message}\n{trace}", ex.Message, ex.StackTrace);
            return RedirectToPage("Index");
        }

        return Page();
    }
}