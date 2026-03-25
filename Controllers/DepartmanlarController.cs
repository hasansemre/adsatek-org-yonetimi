using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize(Roles = "Admin,Yonetici")]
public class DepartmanlarController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmanlarController(IDepartmentService departmentService)
        => _departmentService = departmentService;

    public async Task<IActionResult> Index()
    {
        ViewBag.Hierarchy = await _departmentService.GetHierarchyAsync();
        var departments = await _departmentService.GetAllAsync();
        return View(departments);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        ViewBag.Departments = await _departmentService.GetAllAsync();
        return View(new DepartmentViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(model);
        }
        await _departmentService.CreateAsync(model);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var dept = await _departmentService.GetByIdAsync(id);
        if (dept == null) return NotFound();

        ViewBag.Departments = (await _departmentService.GetAllAsync()).Where(d => d.Id != id).ToList();
        return View(new DepartmentViewModel
        {
            Id = dept.Id,
            Ad = dept.Ad,
            Aciklama = dept.Aciklama,
            UstDepartmanId = dept.UstDepartmanId
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = (await _departmentService.GetAllAsync()).Where(d => d.Id != id).ToList();
            return View(model);
        }
        await _departmentService.UpdateAsync(id, model);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        var (success, error) = await _departmentService.DeleteAsync(id);
        if (!success) TempData["Error"] = error;
        return RedirectToAction("Index");
    }
}
