using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public interface IUserService
{
    Task<List<UserListItemViewModel>> GetAllAsync();
    Task<UserEditViewModel?> GetByIdAsync(string id);
    Task<(bool Success, string? Error)> CreateAsync(UserCreateViewModel model);
    Task<(bool Success, string? Error)> UpdateAsync(UserEditViewModel model);
    Task ToggleActiveAsync(string userId);
    Task<int> GetUserCountAsync();
}
