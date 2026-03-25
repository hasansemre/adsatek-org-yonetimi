using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context) => _context = context;

    public async Task<List<Department>> GetAllAsync()
        => await _context.Departments
            .Include(d => d.UstDepartman)
            .Include(d => d.Kullanicilar)
            .OrderBy(d => d.Ad)
            .ToListAsync();

    public async Task<List<Department>> GetHierarchyAsync()
        => await _context.Departments
            .Include(d => d.AltDepartmanlar)
            .Include(d => d.Kullanicilar)
            .Where(d => d.UstDepartmanId == null)
            .OrderBy(d => d.Ad)
            .ToListAsync();

    public async Task<Department?> GetByIdAsync(int id)
        => await _context.Departments
            .Include(d => d.Kullanicilar)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task CreateAsync(DepartmentViewModel model)
    {
        var dept = new Department
        {
            Ad = model.Ad,
            Aciklama = model.Aciklama,
            UstDepartmanId = model.UstDepartmanId
        };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, DepartmentViewModel model)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return;

        dept.Ad = model.Ad;
        dept.Aciklama = model.Aciklama;
        dept.UstDepartmanId = model.UstDepartmanId;
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.Kullanicilar)
            .Include(d => d.AltDepartmanlar)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept == null) return (false, "Departman bulunamadı");
        if (dept.Kullanicilar.Any()) return (false, "Bu departmanda kullanıcılar var, önce onları taşıyın");
        if (dept.AltDepartmanlar.Any()) return (false, "Bu departmanın alt departmanları var, önce onları silin");

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}
