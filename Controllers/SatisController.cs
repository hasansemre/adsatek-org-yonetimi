using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrgYonetimi.Models;
using OrgYonetimi.Services;

namespace OrgYonetimi.Controllers;

[Authorize(Roles = "Admin,Yonetici")]
public class SatisController : Controller
{
    private readonly ISatisService _satisService;
    private readonly IStokService _stokService;
    private readonly IFirmaService _firmaService;
    private readonly IProjectService _projectService;
    private readonly UserManager<AppUser> _userManager;

    public SatisController(
        ISatisService satisService,
        IStokService stokService,
        IFirmaService firmaService,
        IProjectService projectService,
        UserManager<AppUser> userManager)
    {
        _satisService = satisService;
        _stokService = stokService;
        _firmaService = firmaService;
        _projectService = projectService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var satislar = await _satisService.GetAllAsync();
        return View(satislar);
    }

    public async Task<IActionResult> Ekle()
    {
        ViewBag.StokKartlari = (await _stokService.GetAllAsync()).Where(s => s.IsActive).ToList();
        ViewBag.Firmalar = (await _firmaService.GetAllAsync()).Where(f => f.IsActive).ToList();
        ViewBag.Projeler = await _projectService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(
        int stokKartiId, int musteriFirmaId, int? temsilciFirmaId, int? projectId,
        int miktar, decimal birimFiyat, string? faturaNo, string? aciklama,
        string? tarih, string paraBirimi = "TRY")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var satis = new Satis
        {
            StokKartiId = stokKartiId,
            MusteriFirmaId = musteriFirmaId,
            TemsilciFirmaId = temsilciFirmaId,
            ProjectId = projectId,
            Miktar = miktar,
            Birim = (await _stokService.GetByIdAsync(stokKartiId))?.Birim ?? "adet",
            BirimFiyat = birimFiyat,
            ParaBirimi = paraBirimi,
            FaturaNo = faturaNo,
            Aciklama = aciklama,
            OlusturanId = user.Id,
            Tarih = string.IsNullOrEmpty(tarih) ? DateTime.UtcNow : DateTime.Parse(tarih)
        };

        await _satisService.CreateAsync(satis);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Detay(int id)
    {
        var satis = await _satisService.GetByIdAsync(id);
        if (satis == null) return NotFound();
        return View(satis);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int id, string durum)
    {
        await _satisService.UpdateDurumAsync(id, durum);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sil(int id)
    {
        await _satisService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
