using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize(Roles = "Admin")]
public class KullanicilarController : Controller
{
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;

    public KullanicilarController(IUserService userService, IDepartmentService departmentService)
    {
        _userService = userService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount = await _userService.GetUserCountAsync();
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        ViewBag.Departments = await _departmentService.GetAllAsync();
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(model);
        }
        var (success, error) = await _userService.CreateAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", error!);
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(model);
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Duzenle(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Departments = await _departmentService.GetAllAsync();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(model);
        }
        var (success, error) = await _userService.UpdateAsync(model);
        if (!success)
        {
            ModelState.AddModelError("", error!);
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(model);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AktifDegistir(string id)
    {
        await _userService.ToggleActiveAsync(id);
        return RedirectToAction("Index");
    }
}
