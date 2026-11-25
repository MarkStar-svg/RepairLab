using Microsoft.AspNetCore.Mvc;
using RepairLab.Data;
using RepairLab.Models;
namespace RepairLab.Controllers;

public class HomeController : Controller
{
    private readonly CaseRepository _cases;
    public HomeController(CaseRepository cases) => _cases = cases;

    public IActionResult Index() => View(_cases.GetLatest(4));
    public IActionResult About() => View();
    public IActionResult Contacts() => View(new ContactViewModel());

    [HttpPost]
    public IActionResult Contacts(ContactViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        TempData["Ok"] = "Спасибо! Сообщение отправлено, мы свяжемся с вами по email.";
        return RedirectToAction(nameof(Contacts));
    }

    public IActionResult Error404() { Response.StatusCode = 404; return View(); }
}
