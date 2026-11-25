using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RepairLab.Data;
using RepairLab.Models;
namespace RepairLab.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _users;
    public AccountController(UserRepository users) => _users = users;

    public static Task SignIn(HttpContext http, User u)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()), new(ClaimTypes.Name, u.FullName),
            new(ClaimTypes.Email, u.Email), new(ClaimTypes.Role, u.Role)
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }

    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (_users.GetByEmail(m.Email) != null) ModelState.AddModelError("Email", "Этот email уже зарегистрирован");
        if (!ModelState.IsValid) return View(m);
        var id = _users.Create(m.Email.Trim(), m.FullName.Trim(), PasswordHasher.Hash(m.Password));
        await SignIn(HttpContext, _users.GetById(id));
        return RedirectToAction("Index", "Cabinet");
    }

    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var u = _users.GetByEmail(m.Email.Trim());
        if (u == null || !PasswordHasher.Verify(m.Password, u.PasswordHash))
        {
            ModelState.AddModelError("", "Неверный email или пароль");
            return View(m);
        }
        await SignIn(HttpContext, u);
        return RedirectToAction("Index", "Cabinet");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
