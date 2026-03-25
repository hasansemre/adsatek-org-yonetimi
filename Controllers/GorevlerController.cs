using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize]
public class GorevlerController : Controller
{
    private readonly IGorevService _gorevService;
    private readonly IStokService _stokService;
    private readonly IFirmaService _firmaService;
    private readonly AppDbContext _context;

    public GorevlerController(IGorevService gorevService, IStokService stokService, IFirmaService firmaService, AppDbContext context)
    {
        _gorevService = gorevService;
        _stokService = stokService;
        _firmaService = firmaService;
        _context = context;
    }

    public async Task<IActionResult> Index(GorevFilterModel? filter)
    {
        await _gorevService.CheckOverdueAsync();

        // Calisan kendi görevlerini görür (atanan veya oluşturan)
        if (User.IsInRole("Calisan"))
        {
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id;
            filter ??= new GorevFilterModel();
            filter.KullaniciId = userId;
        }
        // Yönetici kendi departmanındaki görevleri görür
        else if (User.IsInRole("Yonetici"))
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            if (currentUser?.DepartmentId != null)
            {
                var deptUserIds = await _context.Users
                    .Where(u => u.DepartmentId == currentUser.DepartmentId)
                    .Select(u => u.Id)
                    .ToListAsync();
                filter ??= new GorevFilterModel();
                filter.DepartmanKullanicilari = deptUserIds;
            }
        }

        ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        ViewBag.Projects = await _context.Projects.OrderBy(p => p.Ad).ToListAsync();
        ViewBag.Filter = filter ?? new GorevFilterModel();

        var gorevler = await _gorevService.GetAllAsync(filter);
        return View(gorevler);
    }

    [HttpGet]
    public async Task<IActionResult> Ekle(int? projectId)
    {
        ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        ViewBag.Projects = await _context.Projects.OrderBy(p => p.Ad).ToListAsync();
        return View(new GorevCreateViewModel { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(GorevCreateViewModel model, List<IFormFile>? dosyalar)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            ViewBag.Projects = await _context.Projects.OrderBy(p => p.Ad).ToListAsync();
            return View(model);
        }

        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id ?? "";
        var gorevId = await _gorevService.CreateAsync(model, userId, dosyalar);
        return RedirectToAction("Detay", new { id = gorevId });
    }

    public async Task<IActionResult> Detay(int id)
    {
        var gorev = await _gorevService.GetDetayAsync(id);
        if (gorev == null) return RedirectToAction("Index");

        // Calisan sadece kendi görevlerini görebilir (atanan veya oluşturan)
        if (User.IsInRole("Calisan"))
        {
            var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id;
            if (gorev.AtananKullaniciId != userId && gorev.OlusturanId != userId) return RedirectToAction("Index");
        }

        // Stoklu ürünleri ve temsilci firmaları getir
        ViewBag.StokluUrunler = await _stokService.GetStokluUrunlerAsync();
        ViewBag.TemsilciFirmalar = await _firmaService.GetByTipAsync("temsilci");

        return View(gorev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsamaEkle(int gorevId, string asama, string? not, List<IFormFile>? dosyalar)
    {
        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id ?? "";
        await _gorevService.AsamaEkleAsync(gorevId, asama, not, userId, dosyalar);
        return RedirectToAction("Detay", new { id = gorevId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StokKullan(int gorevId, int stokKartiId, int? temsilciFirmaId, decimal miktar, string? aciklama)
    {
        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id ?? "";
        var basarili = await _gorevService.StokKullanAsync(gorevId, stokKartiId, temsilciFirmaId, miktar, aciklama, userId);
        if (!basarili)
        {
            TempData["Error"] = "Yetersiz stok! Kullanmak istediğiniz miktar mevcut stoktan fazla.";
        }
        else
        {
            TempData["Success"] = "Stok kullanımı başarıyla kaydedildi.";
        }
        return RedirectToAction("Detay", new { id = gorevId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumGuncelle(int id, string durum, string? returnUrl)
    {
        await _gorevService.UpdateStatusAsync(id, durum);
        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id, string? returnUrl)
    {
        await _gorevService.DeleteAsync(id);
        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index");
    }
}
