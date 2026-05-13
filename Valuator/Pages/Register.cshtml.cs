using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Auth;

namespace Valuator.Pages;

[AllowAnonymous]
public class RegisterModel(AuthService authService) : PageModel
{
    [BindProperty]
    public string Login { get; set; } = string.Empty;

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string login, string password)
    {
        Login = login;

        try
        {
            authService.Register(login, password);
        }
        catch (UsernameAlreadyTakenException ex)
        {
            Error = ex.Message;
            return Page();
        }

        Claim[] claims = [new(ClaimTypes.Name, login)];
        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToPage("/Index");
    }
}
