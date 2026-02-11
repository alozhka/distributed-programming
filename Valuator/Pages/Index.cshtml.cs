using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(ILogger<IndexModel> logger, ValuatorService valuatorService)
    : PageModel
{
    public IActionResult OnPost(string text)
    {
        logger.LogDebug(text);

        string id = valuatorService.EvaluateText(text);

        return Redirect($"summary?id={id}");
    }
}