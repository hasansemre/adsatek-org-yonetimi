using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public class StokService : IStokService
{
    private readonly AppDbContext _context;

    public StokService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StokKarti>> GetAllAsync()
    {
        return await _context.StokKartlari
            .Where(s => s.IsActive)
            .OrderBy(s => s.Ad)
            .ToListAsync();
    }

    public async Task<StokKarti?> GetByIdAsync(int id)
    {
        return await _context.StokKartlari.FindAsync(id);
    }

    public async Task<StokKarti?> GetByIdWithTedariklerAsync(int id)
    {
        return await _context.StokKartlari
            .Include(s => s.Tedarikler.OrderByDescending(t => t.CreatedAt))
                .ThenInclude(t => t.TedarikFirma)
            .Include(s => s.Tedarikler)
                .ThenInclude(t => t.TemsilciFirma)
            .Include(s => s.Tedarikler)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StokKarti> CreateAsync(StokKarti stokKarti)
    {
        _context.StokKartlari.Add(stokKarti);
        await _context.SaveChangesAsync();
        return stokKarti;
    }

    public async Task UpdateAsync(StokKarti stokKarti)
    {
        _context.StokKartlari.Update(stokKarti);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var stok = await _context.StokKartlari.FindAsync(id);
        if (stok != null)
        {
            stok.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetToplamStokAsync(int stokKartiId)
    {
        var toplam = await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId)
            .SumAsync(h => h.Miktar);
        return toplam;
    }

    public async Task<List<FirmaStokOzet>> GetFirmaBazliStokAsync(int stokKartiId)
    {
        var hareketler = await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId)
            .Include(h => h.TemsilciFirma)
            .ToListAsync();

        var gruplar = hareketler
            .GroupBy(h => h.TemsilciFirmaId)
            .Select(g => new FirmaStokOzet
            {
                TemsilciFirmaId = g.Key,
                FirmaAdi = g.First().TemsilciFirma?.Ad ?? "Genel (Temsilcisiz)",
                ToplamGiris = g.Where(h => h.Miktar > 0).Sum(h => h.Miktar),
                ToplamCikis = g.Where(h => h.Miktar < 0).Sum(h => Math.Abs(h.Miktar))
            })
            .OrderBy(f => f.FirmaAdi)
            .ToList();

        return gruplar;
    }

    public async Task<decimal> GetFirmaStokAsync(int stokKartiId, int temsilciFirmaId)
    {
        return await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId && h.TemsilciFirmaId == temsilciFirmaId)
            .SumAsync(h => h.Miktar);
    }

    public async Task<List<StokHareket>> GetHareketlerAsync(int stokKartiId)
    {
        return await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId)
            .Include(h => h.TemsilciFirma)
            .Include(h => h.Tedarik)
            .Include(h => h.GorevItem)
            .Include(h => h.Olusturan)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task StokGirisiYapAsync(int stokKartiId, int? temsilciFirmaId, decimal miktar, int? tedarikId, string aciklama, string olusturanId)
    {
        var hareket = new StokHareket
        {
            StokKartiId = stokKartiId,
            TemsilciFirmaId = temsilciFirmaId,
            TedarikId = tedarikId,
            Miktar = Math.Abs(miktar), // Her zaman pozitif
            HareketTipi = "giris",
            Aciklama = aciklama,
            OlusturanId = olusturanId
        };
        _context.StokHareketler.Add(hareket);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> StokCikisiYapAsync(int stokKartiId, int? temsilciFirmaId, decimal miktar, int? gorevItemId, string aciklama, string olusturanId)
    {
        // Toplam stok kontrolü (tüm firmalardan)
        decimal mevcutStok;
        if (temsilciFirmaId.HasValue)
        {
            mevcutStok = await GetFirmaStokAsync(stokKartiId, temsilciFirmaId.Value);
        }
        else
        {
            mevcutStok = await GetToplamStokAsync(stokKartiId);
        }

        if (mevcutStok < miktar)
            return false;

        var hareket = new StokHareket
        {
            StokKartiId = stokKartiId,
            TemsilciFirmaId = temsilciFirmaId,
            GorevItemId = gorevItemId,
            Miktar = -Math.Abs(miktar), // Her zaman negatif
            HareketTipi = "cikis",
            Aciklama = aciklama,
            OlusturanId = olusturanId
        };
        _context.StokHareketler.Add(hareket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<StokKarti>> GetStokluUrunlerAsync()
    {
        // Stoğu olan ürünleri getir
        var stokluIds = await _context.StokHareketler
            .GroupBy(h => h.StokKartiId)
            .Where(g => g.Sum(h => h.Miktar) > 0)
            .Select(g => g.Key)
            .ToListAsync();

        return await _context.StokKartlari
            .Where(s => s.IsActive && s.Tip == "urun" && stokluIds.Contains(s.Id))
            .OrderBy(s => s.Ad)
            .ToListAsync();
    }
}
