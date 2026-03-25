using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public class TedarikService : ITedarikService
{
    private readonly AppDbContext _context;
    private readonly IStokService _stokService;

    public TedarikService(AppDbContext context, IStokService stokService)
    {
        _context = context;
        _stokService = stokService;
    }

    public async Task<List<Tedarik>> GetAllAsync()
    {
        return await _context.Tedarikler
            .Include(t => t.StokKarti)
            .Include(t => t.Project)
            .Include(t => t.TedarikFirma)
            .Include(t => t.TemsilciFirma)
            .Include(t => t.Olusturan)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Tedarik>> GetByProjectAsync(int projectId)
    {
        return await _context.Tedarikler
            .Include(t => t.StokKarti)
            .Include(t => t.TedarikFirma)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Tedarik?> GetByIdAsync(int id)
    {
        return await _context.Tedarikler
            .Include(t => t.StokKarti)
            .Include(t => t.Project)
            .Include(t => t.TedarikFirma)
            .Include(t => t.TemsilciFirma)
            .Include(t => t.Olusturan)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tedarik> CreateAsync(Tedarik tedarik)
    {
        tedarik.ToplamTutar = tedarik.Miktar * tedarik.BirimFiyat;
        _context.Tedarikler.Add(tedarik);
        await _context.SaveChangesAsync();
        return tedarik;
    }

    public async Task UpdateDurumAsync(int id, string durum)
    {
        var tedarik = await _context.Tedarikler.FindAsync(id);
        if (tedarik == null) return;

        var eskiDurum = tedarik.Durum;
        tedarik.Durum = durum;
        await _context.SaveChangesAsync();

        // Teslim alındı olduğunda stok girişi yap
        if (durum == "teslim_alindi" && eskiDurum != "teslim_alindi")
        {
            await _stokService.StokGirisiYapAsync(
                tedarik.StokKartiId,
                tedarik.TemsilciFirmaId,
                tedarik.Miktar,
                tedarik.Id,
                $"Tedarik #{tedarik.Id} - Teslim alındı",
                tedarik.OlusturanId
            );
        }
    }

    public async Task DeleteAsync(int id)
    {
        var tedarik = await _context.Tedarikler.FindAsync(id);
        if (tedarik != null)
        {
            _context.Tedarikler.Remove(tedarik);
            await _context.SaveChangesAsync();
        }
    }
}
