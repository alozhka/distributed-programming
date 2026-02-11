using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, ValuatorService valuatorService)
    : PageModel
{
    public IActionResult OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return RedirectToPage("Index");
        }

        logger.LogDebug("Written text: {text}", text);

        string id = valuatorService.EvaluateText(text);
        return Redirect($"summary?id={id}");
    }
}