using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairLab.Data;
using RepairLab.Models;
namespace RepairLab.Controllers;

[Authorize]
public class CabinetController : Controller
{
    private readonly CaseRepository _cases;
    private readonly UserRepository _users;
    public CabinetController(CaseRepository cases, UserRepository users) { _cases = cases; _users = users; }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    public IActionResult Index() { ViewBag.Count = _cases.CountByUser(UserId); return View(); }
    public IActionResult MyItems() => View(_cases.GetByUser(UserId));

    public IActionResult Create() => View(new RepairCase());

    [HttpPost]
    public IActionResult Create(RepairCase m)
    {
        if (!ModelState.IsValid) return View(m);
        m.UserId = UserId;
        _cases.Create(m);
        TempData["Ok"] = "Заявка добавлена";
        return RedirectToAction(nameof(MyItems));
    }

    public IActionResult Edit(int id)
    {
        var item = _cases.GetOwned(id, UserId);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost]
    public IActionResult Edit(RepairCase m)
    {
        if (!ModelState.IsValid) return View(m);
        m.UserId = UserId;
        if (!_cases.Update(m)) return NotFound();   // чужая запись → не обновится
        TempData["Ok"] = "Изменения сохранены";
        return RedirectToAction(nameof(MyItems));
    }

    public IActionResult Delete(int id)
    {
        var item = _cases.GetOwned(id, UserId);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_cases.Delete(id, UserId)) return NotFound();
        TempData["Ok"] = "Заявка удалена";
        return RedirectToAction(nameof(MyItems));
    }

    public IActionResult Profile()
    {
        var u = _users.GetById(UserId);
        return View(new ProfileViewModel { FullName = u.FullName, Email = u.Email, CreatedAt = u.CreatedAt });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel m)
    {
        var u = _users.GetById(UserId);
        m.CreatedAt = u.CreatedAt;
        if (_users.EmailTakenByOther(m.Email, u.Id)) ModelState.AddModelError("Email", "Email уже занят");
        if (!ModelState.IsValid) return View(m);
        _users.UpdateProfile(u.Id, m.FullName.Trim(), m.Email.Trim());
        await HttpContext.SignOutAsync();
        await AccountController.SignIn(HttpContext, _users.GetById(u.Id));
        TempData["Ok"] = "Профиль обновлён";
        return RedirectToAction(nameof(Profile));
    }

    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    public IActionResult ChangePassword(ChangePasswordViewModel m)
    {
        var u = _users.GetById(UserId);
        if (!PasswordHasher.Verify(m.OldPassword ?? "", u.PasswordHash)) ModelState.AddModelError("OldPassword", "Старый пароль неверный");
        if (!ModelState.IsValid) return View(m);
        _users.UpdatePassword(u.Id, PasswordHasher.Hash(m.NewPassword));
        TempData["Ok"] = "Пароль изменён";
        return RedirectToAction(nameof(Index));
    }
}
