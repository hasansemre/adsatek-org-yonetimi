using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public class SatisService : ISatisService
{
    private readonly AppDbContext _context;
    private readonly IStokService _stokService;

    public SatisService(AppDbContext context, IStokService stokService)
    {
        _context = context;
        _stokService = stokService;
    }

    public async Task<List<Satis>> GetAllAsync()
    {
        return await _context.Satislar
            .Include(s => s.StokKarti)
            .Include(s => s.MusteriFirma)
            .Include(s => s.TemsilciFirma)
            .Include(s => s.Project)
            .Include(s => s.Olusturan)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Satis?> GetByIdAsync(int id)
    {
        return await _context.Satislar
            .Include(s => s.StokKarti)
            .Include(s => s.MusteriFirma)
            .Include(s => s.TemsilciFirma)
            .Include(s => s.Project)
            .Include(s => s.Olusturan)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Satis> CreateAsync(Satis satis)
    {
        satis.ToplamTutar = satis.Miktar * satis.BirimFiyat;
        _context.Satislar.Add(satis);
        await _context.SaveChangesAsync();
        return satis;
    }

    public async Task UpdateDurumAsync(int id, string durum)
    {
        var satis = await _context.Satislar.FindAsync(id);
        if (satis == null) return;

        var eskiDurum = satis.Durum;
        satis.Durum = durum;
        await _context.SaveChangesAsync();

        // Teslim edildi olduğunda stoktan düş (ürün ise)
        if (durum == "teslim_edildi" && eskiDurum != "teslim_edildi")
        {
            var stokKarti = await _stokService.GetByIdAsync(satis.StokKartiId);
            if (stokKarti != null && stokKarti.Tip == "urun")
            {
                await _stokService.StokCikisiYapAsync(
                    satis.StokKartiId,
                    satis.TemsilciFirmaId,
                    satis.Miktar,
                    null,
                    $"Satış #{satis.Id} - Fatura: {satis.FaturaNo ?? "-"}",
                    satis.OlusturanId
                );
            }
        }
    }

    public async Task DeleteAsync(int id)
    {
        var satis = await _context.Satislar.FindAsync(id);
        if (satis != null)
        {
            _context.Satislar.Remove(satis);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetToplamSatisTutarAsync()
    {
        return await _context.Satislar
            .Where(s => s.Durum != "iptal")
            .SumAsync(s => s.ToplamTutar);
    }

    public async Task<List<Satis>> GetByStokKartiAsync(int stokKartiId)
    {
        return await _context.Satislar
            .Include(s => s.MusteriFirma)
            .Include(s => s.TemsilciFirma)
            .Include(s => s.Project)
            .Where(s => s.StokKartiId == stokKartiId)
            .OrderByDescending(s => s.Tarih)
            .ToListAsync();
    }
}
