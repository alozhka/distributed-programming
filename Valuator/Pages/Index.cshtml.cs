using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, ValuatorService valuatorService)
    : PageModel
{
    public async Task<IActionResult> OnPost(string text, string country)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(country))
        {
            return RedirectToPage("Index");
        }

        logger.LogDebug("Written text: {text} from country {country}", text, country);

        string id = await valuatorService.EvaluateText(text, country);
        return Redirect($"summary?id={id}");
    }
}