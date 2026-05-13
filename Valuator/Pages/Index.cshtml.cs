using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

[Authorize]
public class IndexModel(ILogger<IndexModel> logger, ValuatorService valuatorService)
    : PageModel
{
    public async Task<IActionResult> OnPost(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return RedirectToPage("Index");
        }

        logger.LogDebug("Written text: {text}", text);

        string id = await valuatorService.EvaluateText(text, User.Identity!.Name!);
        return Redirect($"summary?id={id}");
    }
}