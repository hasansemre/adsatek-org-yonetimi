using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize]
public class FirmalarController : Controller
{
    private readonly IFirmaService _firmaService;
    private readonly AppDbContext _context;

    public FirmalarController(IFirmaService firmaService, AppDbContext context)
    {
        _firmaService = firmaService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? tip)
    {
        var firmalar = string.IsNullOrEmpty(tip)
            ? await _firmaService.GetAllAsync()
            : await _firmaService.GetByTipAsync(tip);
        ViewBag.AktifTip = tip;
        return View(firmalar);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var firma = await _firmaService.GetByIdAsync(id);
        if (firma == null) return NotFound();

        // Firma ile ilgili tedarikler
        var tedarikler = await _context.Tedarikler
            .Include(t => t.StokKarti)
            .Include(t => t.Project)
            .Include(t => t.TedarikFirma)
            .Include(t => t.TemsilciFirma)
            .Where(t => t.TedarikFirmaId == id || t.TemsilciFirmaId == id)
            .OrderByDescending(t => t.Tarih)
            .ToListAsync();

        ViewBag.Tedarikler = tedarikler;

        // Toplam harcama
        var toplamHarcama = tedarikler
            .Where(t => t.Durum == "teslim_alindi")
            .Sum(t => t.ToplamTutar);
        ViewBag.ToplamHarcama = toplamHarcama;

        var bekleyenTutar = tedarikler
            .Where(t => t.Durum == "beklemede" || t.Durum == "onaylandi")
            .Sum(t => t.ToplamTutar);
        ViewBag.BekleyenTutar = bekleyenTutar;

        // Firma temsilci ise stok bilgisi
        if (firma.Tip == "temsilci")
        {
            var stoklar = await _context.StokHareketler
                .Where(h => h.TemsilciFirmaId == id)
                .Include(h => h.StokKarti)
                .GroupBy(h => new { h.StokKartiId, h.StokKarti.Ad, h.StokKarti.Birim })
                .Select(g => new { UrunAdi = g.Key.Ad, Birim = g.Key.Birim, Adet = (int)g.Sum(h => h.Miktar) })
                .Where(s => s.Adet > 0)
                .ToListAsync();
            ViewBag.Stoklar = stoklar.Select(s => new { s.UrunAdi, s.Adet, s.Birim }).ToList();
        }

        return View(firma);
    }

    [Authorize(Roles = "Admin,Yonetici")]
    public IActionResult Ekle()
    {
        return View(new FirmaViewModel());
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(FirmaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var firma = new Firma
        {
            Ad = model.Ad,
            Tip = model.Tip,
            Telefon = model.Telefon,
            Email = model.Email,
            Adres = model.Adres,
            YetkiliKisi = model.YetkiliKisi,
            VergiNo = model.VergiNo,
            Aciklama = model.Aciklama
        };

        await _firmaService.CreateAsync(firma);
        TempData["Success"] = "Firma başarıyla eklendi.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Duzenle(int id)
    {
        var firma = await _firmaService.GetByIdAsync(id);
        if (firma == null) return NotFound();

        var model = new FirmaViewModel
        {
            Id = firma.Id,
            Ad = firma.Ad,
            Tip = firma.Tip,
            Telefon = firma.Telefon,
            Email = firma.Email,
            Adres = firma.Adres,
            YetkiliKisi = firma.YetkiliKisi,
            VergiNo = firma.VergiNo,
            Aciklama = firma.Aciklama
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(FirmaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var firma = await _firmaService.GetByIdAsync(model.Id);
        if (firma == null) return NotFound();

        firma.Ad = model.Ad;
        firma.Tip = model.Tip;
        firma.Telefon = model.Telefon;
        firma.Email = model.Email;
        firma.Adres = model.Adres;
        firma.YetkiliKisi = model.YetkiliKisi;
        firma.VergiNo = model.VergiNo;
        firma.Aciklama = model.Aciklama;

        await _firmaService.UpdateAsync(firma);
        TempData["Success"] = "Firma başarıyla güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sil(int id)
    {
        await _firmaService.DeleteAsync(id);
        TempData["Success"] = "Firma pasife alındı.";
        return RedirectToAction("Index");
    }
}
