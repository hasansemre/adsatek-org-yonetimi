using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public class GorevService : IGorevService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IStokService _stokService;

    public GorevService(AppDbContext context, IWebHostEnvironment env, IStokService stokService)
    {
        _context = context;
        _env = env;
        _stokService = stokService;
    }

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",
        ".pdf",
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods", ".odp"
    };

    private string GetDosyaTipi(string ext)
    {
        ext = ext.ToLowerInvariant();
        if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg") return "image";
        if (ext is ".pdf") return "pdf";
        return "office";
    }

    private async Task<List<GorevDosya>> DosyalariKaydetAsync(List<IFormFile> dosyalar, int gorevId, int? asamaId, string kullaniciId)
    {
        var sonuc = new List<GorevDosya>();
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "gorevler", gorevId.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var dosya in dosyalar)
        {
            if (dosya.Length == 0) continue;
            var ext = Path.GetExtension(dosya.FileName);
            if (!AllowedExtensions.Contains(ext)) continue;
            if (dosya.Length > 10 * 1024 * 1024) continue; // max 10MB

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dosya.CopyToAsync(stream);
            }

            var gorevDosya = new GorevDosya
            {
                GorevItemId = gorevId,
                GorevAsamaId = asamaId,
                DosyaAdi = dosya.FileName,
                DosyaYolu = $"/uploads/gorevler/{gorevId}/{uniqueName}",
                DosyaTipi = GetDosyaTipi(ext),
                DosyaBoyutu = dosya.Length,
                YukleyenKullaniciId = kullaniciId
            };
            _context.GorevDosyalar.Add(gorevDosya);
            sonuc.Add(gorevDosya);
        }

        return sonuc;
    }

    public async Task<List<GorevItem>> GetAllAsync(GorevFilterModel? filter = null)
    {
        var query = _context.GorevItems
            .Include(g => g.AtananKullanici)
            .Include(g => g.Project)
            .Include(g => g.Olusturan)
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Durum))
                query = query.Where(g => g.Durum == filter.Durum);
            if (!string.IsNullOrEmpty(filter.Oncelik))
                query = query.Where(g => g.Oncelik == filter.Oncelik);
            if (filter.ProjectId.HasValue)
                query = query.Where(g => g.ProjectId == filter.ProjectId);
            if (!string.IsNullOrEmpty(filter.AtananKullaniciId))
                query = query.Where(g => g.AtananKullaniciId == filter.AtananKullaniciId);
            if (!string.IsNullOrEmpty(filter.KullaniciId))
                query = query.Where(g => g.AtananKullaniciId == filter.KullaniciId || g.OlusturanId == filter.KullaniciId);
            if (filter.DepartmanKullanicilari != null && filter.DepartmanKullanicilari.Any())
                query = query.Where(g =>
                    filter.DepartmanKullanicilari.Contains(g.AtananKullaniciId ?? "") ||
                    filter.DepartmanKullanicilari.Contains(g.OlusturanId));
        }

        return await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
    }

    public async Task<List<GorevItem>> GetByProjectAsync(int projectId)
        => await _context.GorevItems
            .Include(g => g.AtananKullanici)
            .Where(g => g.ProjectId == projectId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<List<GorevItem>> GetByUserAsync(string userId)
        => await _context.GorevItems
            .Include(g => g.Project)
            .Where(g => g.AtananKullaniciId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<GorevItem?> GetByIdAsync(int id)
        => await _context.GorevItems
            .Include(g => g.AtananKullanici)
            .Include(g => g.Project)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<GorevItem?> GetDetayAsync(int id)
        => await _context.GorevItems
            .Include(g => g.AtananKullanici)
            .Include(g => g.Olusturan)
            .Include(g => g.Project)
            .Include(g => g.Dosyalar.OrderByDescending(d => d.CreatedAt))
                .ThenInclude(d => d.YukleyenKullanici)
            .Include(g => g.Asamalar.OrderByDescending(a => a.CreatedAt))
                .ThenInclude(a => a.YapanKullanici)
            .Include(g => g.Asamalar)
                .ThenInclude(a => a.Dosyalar)
            .Include(g => g.StokKullanimlari.OrderByDescending(k => k.CreatedAt))
                .ThenInclude(k => k.StokKarti)
            .Include(g => g.StokKullanimlari)
                .ThenInclude(k => k.TemsilciFirma)
            .Include(g => g.StokKullanimlari)
                .ThenInclude(k => k.Kullanan)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task AsamaEkleAsync(int gorevId, string asama, string? not, string yapanKullaniciId, List<IFormFile>? dosyalar = null)
    {
        var gorev = await _context.GorevItems.FindAsync(gorevId);
        if (gorev == null) return;

        var yeniAsama = new GorevAsama
        {
            GorevItemId = gorevId,
            Asama = asama,
            Not = not,
            YapanKullaniciId = yapanKullaniciId
        };
        _context.GorevAsamalar.Add(yeniAsama);
        await _context.SaveChangesAsync(); // Save to get asamaId

        // Aşamaya göre durum güncelle
        if (asama == "isleme_alindi")
            gorev.Durum = "devam";
        else if (asama == "tamamlandi")
        {
            gorev.Durum = "tamamlandi";
            gorev.TamamlanmaTarihi = DateTime.UtcNow;
        }
        else if (asama == "reddedildi")
            gorev.Durum = "bekliyor";

        // Dosyaları kaydet
        if (dosyalar != null && dosyalar.Any())
        {
            await DosyalariKaydetAsync(dosyalar, gorevId, yeniAsama.Id, yapanKullaniciId);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> CreateAsync(GorevCreateViewModel model, string olusturanId, List<IFormFile>? dosyalar = null)
    {
        var gorev = new GorevItem
        {
            Baslik = model.Baslik,
            Aciklama = model.Aciklama,
            Oncelik = model.Oncelik,
            SonTarih = model.SonTarih,
            ProjectId = model.ProjectId,
            AtananKullaniciId = model.AtananKullaniciId,
            OlusturanId = olusturanId
        };
        _context.GorevItems.Add(gorev);
        await _context.SaveChangesAsync(); // Save to get gorevId

        // Dosyaları kaydet
        if (dosyalar != null && dosyalar.Any())
        {
            await DosyalariKaydetAsync(dosyalar, gorev.Id, null, olusturanId);
            await _context.SaveChangesAsync();
        }

        return gorev.Id;
    }

    public async Task UpdateStatusAsync(int id, string durum)
    {
        var gorev = await _context.GorevItems.FindAsync(id);
        if (gorev == null) return;

        gorev.Durum = durum;
        if (durum == "tamamlandi")
            gorev.TamamlanmaTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var gorev = await _context.GorevItems.FindAsync(id);
        if (gorev == null) return;

        // Dosyaları sil
        var dosyalar = await _context.GorevDosyalar.Where(d => d.GorevItemId == id).ToListAsync();
        foreach (var d in dosyalar)
        {
            var fullPath = Path.Combine(_env.WebRootPath, d.DosyaYolu.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        // Klasörü sil
        var dir = Path.Combine(_env.WebRootPath, "uploads", "gorevler", id.ToString());
        if (Directory.Exists(dir)) Directory.Delete(dir, true);

        _context.GorevItems.Remove(gorev);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> StokKullanAsync(int gorevId, int stokKartiId, int? temsilciFirmaId, decimal miktar, string? aciklama, string kullananId)
    {
        // Stok çıkışı yap
        var basarili = await _stokService.StokCikisiYapAsync(
            stokKartiId, temsilciFirmaId, miktar, gorevId,
            aciklama ?? $"Görev #{gorevId} için kullanıldı", kullananId
        );

        if (!basarili) return false;

        // Kullanım kaydı oluştur
        var kullanim = new GorevStokKullanim
        {
            GorevItemId = gorevId,
            StokKartiId = stokKartiId,
            TemsilciFirmaId = temsilciFirmaId,
            Miktar = miktar,
            Aciklama = aciklama,
            KullananId = kullananId
        };
        _context.GorevStokKullanimlari.Add(kullanim);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GorevStokKullanim>> GetStokKullanimlariAsync(int gorevId)
    {
        return await _context.GorevStokKullanimlari
            .Where(k => k.GorevItemId == gorevId)
            .Include(k => k.StokKarti)
            .Include(k => k.TemsilciFirma)
            .Include(k => k.Kullanan)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    public async Task CheckOverdueAsync()
    {
        var overdue = await _context.GorevItems
            .Where(g => g.Durum != "tamamlandi" && g.Durum != "gecikti" && g.SonTarih < DateTime.UtcNow)
            .ToListAsync();

        foreach (var g in overdue)
            g.Durum = "gecikti";

        if (overdue.Any())
            await _context.SaveChangesAsync();
    }
}
