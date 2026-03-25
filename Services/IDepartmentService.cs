using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();
    Task<List<Department>> GetHierarchyAsync();
    Task<Department?> GetByIdAsync(int id);
    Task CreateAsync(DepartmentViewModel model);
    Task UpdateAsync(int id, DepartmentViewModel model);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}
