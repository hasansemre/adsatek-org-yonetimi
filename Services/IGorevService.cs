using Microsoft.AspNetCore.Http;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public interface IGorevService
{
    Task<List<GorevItem>> GetAllAsync(GorevFilterModel? filter = null);
    Task<List<GorevItem>> GetByProjectAsync(int projectId);
    Task<List<GorevItem>> GetByUserAsync(string userId);
    Task<GorevItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(GorevCreateViewModel model, string olusturanId, List<IFormFile>? dosyalar = null);
    Task UpdateStatusAsync(int id, string durum);
    Task DeleteAsync(int id);
    Task CheckOverdueAsync();
    Task<GorevItem?> GetDetayAsync(int id);
    Task AsamaEkleAsync(int gorevId, string asama, string? not, string yapanKullaniciId, List<IFormFile>? dosyalar = null);
    Task<bool> StokKullanAsync(int gorevId, int stokKartiId, int? temsilciFirmaId, decimal miktar, string? aciklama, string kullananId);
    Task<List<GorevStokKullanim>> GetStokKullanimlariAsync(int gorevId);
}
