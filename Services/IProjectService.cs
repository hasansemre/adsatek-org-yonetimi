using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(int id);
    Task CreateAsync(ProjectViewModel model);
    Task UpdateAsync(int id, ProjectViewModel model);
    Task DeleteAsync(int id);
}
