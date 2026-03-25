using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgYonetimi.Models;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize(Roles = "Admin,Yonetici")]
public class StokController : Controller
{
    private readonly IStokService _stokService;
    private readonly ISatisService _satisService;

    public StokController(IStokService stokService, ISatisService satisService)
    {
        _stokService = stokService;
        _satisService = satisService;
    }

    public async Task<IActionResult> Index()
    {
        var stokKartlari = await _stokService.GetAllAsync();
        var stokBilgileri = new Dictionary<int, decimal>();
        foreach (var sk in stokKartlari)
        {
            stokBilgileri[sk.Id] = await _stokService.GetToplamStokAsync(sk.Id);
        }
        ViewBag.StokBilgileri = stokBilgileri;
        return View(stokKartlari);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var stok = await _stokService.GetByIdWithTedariklerAsync(id);
        if (stok == null) return NotFound();

        ViewBag.ToplamStok = await _stokService.GetToplamStokAsync(id);
        ViewBag.FirmaStoklar = await _stokService.GetFirmaBazliStokAsync(id);
        ViewBag.Hareketler = await _stokService.GetHareketlerAsync(id);
        ViewBag.Satislar = await _satisService.GetByStokKartiAsync(id);
        return View(stok);
    }

    public IActionResult Ekle()
    {
        return View(new StokKartiViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(StokKartiViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var stok = new StokKarti
        {
            Ad = model.Ad,
            Kod = model.Kod,
            Aciklama = model.Aciklama,
            Birim = model.Birim,
            Kategori = model.Kategori,
            Tip = model.Tip,
            MinStok = model.MinStok,
            BirimFiyat = model.BirimFiyat,
            ParaBirimi = model.ParaBirimi
        };

        await _stokService.CreateAsync(stok);
        TempData["Success"] = "Stok kartı başarıyla oluşturuldu.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var stok = await _stokService.GetByIdAsync(id);
        if (stok == null) return NotFound();

        var model = new StokKartiViewModel
        {
            Id = stok.Id,
            Ad = stok.Ad,
            Kod = stok.Kod,
            Aciklama = stok.Aciklama,
            Birim = stok.Birim,
            Kategori = stok.Kategori,
            Tip = stok.Tip,
            MinStok = stok.MinStok,
            BirimFiyat = stok.BirimFiyat,
            ParaBirimi = stok.ParaBirimi
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(StokKartiViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var stok = await _stokService.GetByIdAsync(model.Id);
        if (stok == null) return NotFound();

        stok.Ad = model.Ad;
        stok.Kod = model.Kod;
        stok.Aciklama = model.Aciklama;
        stok.Birim = model.Birim;
        stok.Kategori = model.Kategori;
        stok.Tip = model.Tip;
        stok.MinStok = model.MinStok;
        stok.BirimFiyat = model.BirimFiyat;
        stok.ParaBirimi = model.ParaBirimi;

        await _stokService.UpdateAsync(stok);
        TempData["Success"] = "Stok kartı başarıyla güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sil(int id)
    {
        await _stokService.DeleteAsync(id);
        TempData["Success"] = "Stok kartı pasife alındı.";
        return RedirectToAction("Index");
    }
}
