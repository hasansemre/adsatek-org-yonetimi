using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public interface IFirmaService
{
    Task<List<Firma>> GetAllAsync();
    Task<List<Firma>> GetByTipAsync(string tip);
    Task<Firma?> GetByIdAsync(int id);
    Task<Firma> CreateAsync(Firma firma);
    Task UpdateAsync(Firma firma);
    Task DeleteAsync(int id);
}
