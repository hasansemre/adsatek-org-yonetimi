using OrgYonetimi.Models;

namespace OrgYonetimi.Services;

public interface ITedarikService
{
    Task<List<Tedarik>> GetAllAsync();
    Task<List<Tedarik>> GetByProjectAsync(int projectId);
    Task<Tedarik?> GetByIdAsync(int id);
    Task<Tedarik> CreateAsync(Tedarik tedarik);
    Task UpdateDurumAsync(int id, string durum);
    Task DeleteAsync(int id);
}
