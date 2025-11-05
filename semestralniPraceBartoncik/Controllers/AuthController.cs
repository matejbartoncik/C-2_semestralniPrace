using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace semestralniPraceBartoncik.Controllers;

public class AuthController : Controller
{
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email)
    {
      if (string.Equals(email, "admin@demo.cz", StringComparison.OrdinalIgnoreCase))
        {
            var claims = new[] 
            { 
      new Claim(ClaimTypes.Name, "Admin"), 
    new Claim(ClaimTypes.Role, "Admin") 
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
   await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return RedirectToAction("Index", "Home");
        }
        
  ModelState.AddModelError("", "Neplatný e-mail. Zkuste: admin@demo.cz");
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
