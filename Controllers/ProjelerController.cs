using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize]
public class ProjelerController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IGorevService _gorevService;
    private readonly AppDbContext _context;

    public ProjelerController(IProjectService projectService, IGorevService gorevService, AppDbContext context)
    {
        _projectService = projectService;
        _gorevService = gorevService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var projects = await _projectService.GetAllAsync();

        // Yönetici ve Çalışan kendi departmanlarındaki projeleri görür
        if (!User.IsInRole("Admin"))
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            if (currentUser?.DepartmentId != null)
            {
                var deptUserIds = await _context.Users
                    .Where(u => u.DepartmentId == currentUser.DepartmentId)
                    .Select(u => u.Id)
                    .ToListAsync();

                projects = projects.Where(p =>
                    deptUserIds.Contains(p.YoneticiId ?? "") ||
                    p.Gorevler.Any(g => deptUserIds.Contains(g.AtananKullaniciId ?? "") || deptUserIds.Contains(g.OlusturanId))
                ).ToList();
            }
        }

        return View(projects);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Yonetici)
            .Include(p => p.Gorevler).ThenInclude(g => g.AtananKullanici)
            .Include(p => p.Zimmetler).ThenInclude(z => z.TeslimAlanKisi)
            .Include(p => p.Zimmetler).ThenInclude(z => z.Olusturan)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();

        ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        ViewBag.Projeler = await _context.Projects.Where(p => p.Durum == "aktif").OrderBy(p => p.Ad).ToListAsync();
        return View(project);
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        return View(new ProjectViewModel());
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(ProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            return View(model);
        }
        await _projectService.CreateAsync(model);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null) return NotFound();

        ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
        return View(new ProjectViewModel
        {
            Id = project.Id,
            Ad = project.Ad,
            Aciklama = project.Aciklama,
            Durum = project.Durum,
            BaslangicTarihi = project.BaslangicTarihi,
            BitisTarihi = project.BitisTarihi,
            YoneticiId = project.YoneticiId
        });
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, ProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            return View(model);
        }
        await _projectService.UpdateAsync(id, model);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(int id)
    {
        await _projectService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZimmetEkle(int projectId, string ad, string? aciklama, string? seriNo, int adet, string? teslimAlanKisiId)
    {
        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id ?? "";
        var zimmet = new Zimmet
        {
            ProjectId = projectId,
            Ad = ad,
            Aciklama = aciklama,
            SeriNo = seriNo,
            Adet = adet > 0 ? adet : 1,
            TeslimAlanKisiId = string.IsNullOrEmpty(teslimAlanKisiId) ? null : teslimAlanKisiId,
            TeslimTarihi = DateTime.UtcNow,
            OlusturanId = userId
        };
        _context.Zimmetler.Add(zimmet);
        await _context.SaveChangesAsync();
        return RedirectToAction("Detay", new { id = projectId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZimmetDurumGuncelle(int id, string durum, int projectId)
    {
        var zimmet = await _context.Zimmetler.FindAsync(id);
        if (zimmet != null)
        {
            zimmet.Durum = durum;
            if (durum == "iade_edildi") zimmet.IadeTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Detay", new { id = projectId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZimmetGuncelle(int id, int projectId, string? teslimAlanKisiId, int? yeniProjectId)
    {
        var zimmet = await _context.Zimmetler.FindAsync(id);
        if (zimmet != null)
        {
            zimmet.TeslimAlanKisiId = string.IsNullOrEmpty(teslimAlanKisiId) ? null : teslimAlanKisiId;
            if (yeniProjectId.HasValue && yeniProjectId.Value > 0)
                zimmet.ProjectId = yeniProjectId.Value;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Detay", new { id = yeniProjectId ?? projectId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZimmetSil(int id, int projectId)
    {
        var zimmet = await _context.Zimmetler.FindAsync(id);
        if (zimmet != null)
        {
            _context.Zimmetler.Remove(zimmet);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Detay", new { id = projectId });
    }

    [Authorize(Roles = "Admin,Yonetici")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GorevEkle(GorevCreateViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction("Detay", new { id = model.ProjectId });

        var userId = _context.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name)?.Id ?? "";
        await _gorevService.CreateAsync(model, userId);
        return RedirectToAction("Detay", new { id = model.ProjectId });
    }
}
