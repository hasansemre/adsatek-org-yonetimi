using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgYonetimi.Models;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize(Roles = "Admin,Yonetici")]
public class TedarikController : Controller
{
    private readonly ITedarikService _tedarikService;
    private readonly IStokService _stokService;
    private readonly IFirmaService _firmaService;
    private readonly IProjectService _projectService;

    public TedarikController(
        ITedarikService tedarikService,
        IStokService stokService,
        IFirmaService firmaService,
        IProjectService projectService)
    {
        _tedarikService = tedarikService;
        _stokService = stokService;
        _firmaService = firmaService;
        _projectService = projectService;
    }

    public async Task<IActionResult> Index()
    {
        var tedarikler = await _tedarikService.GetAllAsync();
        return View(tedarikler);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var tedarik = await _tedarikService.GetByIdAsync(id);
        if (tedarik == null) return NotFound();
        return View(tedarik);
    }

    public async Task<IActionResult> Ekle()
    {
        await LoadDropdownsAsync();
        return View(new TedarikViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(TedarikViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var tedarik = new Tedarik
        {
            StokKartiId = model.StokKartiId,
            ProjectId = model.ProjectId,
            TedarikFirmaId = model.TedarikFirmaId,
            TemsilciFirmaId = model.TemsilciFirmaId,
            Miktar = model.Miktar,
            Birim = model.Birim,
            BirimFiyat = model.BirimFiyat,
            ParaBirimi = model.ParaBirimi,
            FaturaNo = model.FaturaNo,
            Tarih = model.Tarih,
            Aciklama = model.Aciklama,
            OlusturanId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
        };

        await _tedarikService.CreateAsync(tedarik);
        TempData["Success"] = "Tedarik kaydı başarıyla oluşturuldu.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int id, string durum)
    {
        await _tedarikService.UpdateDurumAsync(id, durum);
        TempData["Success"] = "Tedarik durumu güncellendi.";
        return RedirectToAction("Detay", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sil(int id)
    {
        await _tedarikService.DeleteAsync(id);
        TempData["Success"] = "Tedarik kaydı silindi.";
        return RedirectToAction("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.StokKartlari = await _stokService.GetAllAsync();
        ViewBag.TedarikFirmalar = await _firmaService.GetByTipAsync("tedarikci");
        ViewBag.TemsilciFirmalar = await _firmaService.GetByTipAsync("temsilci");
        ViewBag.Projeler = await _projectService.GetAllAsync();
    }
}
