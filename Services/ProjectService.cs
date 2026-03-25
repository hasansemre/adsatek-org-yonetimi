using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context) => _context = context;

    public async Task<List<Project>> GetAllAsync()
        => await _context.Projects
            .Include(p => p.Yonetici)
            .Include(p => p.Gorevler)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Project?> GetByIdAsync(int id)
        => await _context.Projects
            .Include(p => p.Yonetici)
            .Include(p => p.Gorevler).ThenInclude(g => g.AtananKullanici)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task CreateAsync(ProjectViewModel model)
    {
        var project = new Project
        {
            Ad = model.Ad,
            Aciklama = model.Aciklama,
            Durum = model.Durum,
            BaslangicTarihi = model.BaslangicTarihi,
            BitisTarihi = model.BitisTarihi,
            YoneticiId = model.YoneticiId
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, ProjectViewModel model)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return;

        project.Ad = model.Ad;
        project.Aciklama = model.Aciklama;
        project.Durum = model.Durum;
        project.BaslangicTarihi = model.BaslangicTarihi;
        project.BitisTarihi = model.BitisTarihi;
        project.YoneticiId = model.YoneticiId;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }
}
