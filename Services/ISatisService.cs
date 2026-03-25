using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public interface ISatisService
{
    Task<List<Satis>> GetAllAsync();
    Task<Satis?> GetByIdAsync(int id);
    Task<Satis> CreateAsync(Satis satis);
    Task UpdateDurumAsync(int id, string durum);
    Task DeleteAsync(int id);
    Task<decimal> GetToplamSatisTutarAsync();
    Task<List<Satis>> GetByStokKartiAsync(int stokKartiId);
}
