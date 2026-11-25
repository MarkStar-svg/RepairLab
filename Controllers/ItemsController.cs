using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairLab.Data;
namespace RepairLab.Controllers;

public class ItemsController : Controller
{
    private readonly CaseRepository _cases;
    public ItemsController(CaseRepository cases) => _cases = cases;

    public IActionResult Index(string q) { ViewData["q"] = q; return View(_cases.GetAll(q)); }

    public IActionResult Details(int id)
    {
        var item = _cases.GetById(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public IActionResult AdminDelete(int id) { _cases.DeleteAny(id); return RedirectToAction(nameof(Index)); }
}
