using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public class FirmaService : IFirmaService
{
    private readonly AppDbContext _context;

    public FirmaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Firma>> GetAllAsync()
    {
        return await _context.Firmalar
            .OrderBy(f => f.Tip)
            .ThenBy(f => f.Ad)
            .ToListAsync();
    }

    public async Task<List<Firma>> GetByTipAsync(string tip)
    {
        return await _context.Firmalar
            .Where(f => f.Tip == tip && f.IsActive)
            .OrderBy(f => f.Ad)
            .ToListAsync();
    }

    public async Task<Firma?> GetByIdAsync(int id)
    {
        return await _context.Firmalar.FindAsync(id);
    }

    public async Task<Firma> CreateAsync(Firma firma)
    {
        _context.Firmalar.Add(firma);
        await _context.SaveChangesAsync();
        return firma;
    }

    public async Task UpdateAsync(Firma firma)
    {
        _context.Firmalar.Update(firma);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var firma = await _context.Firmalar.FindAsync(id);
        if (firma != null)
        {
            firma.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
