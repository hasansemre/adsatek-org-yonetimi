using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public UserService(UserManager<AppUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<List<UserListItemViewModel>> GetAllAsync()
    {
        var users = await _context.Users
            .Include(u => u.Department)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                DepartmentAd = u.Department?.Ad,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }
        return result;
    }

    public async Task<UserEditViewModel?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "",
            DepartmentId = user.DepartmentId,
            IsActive = user.IsActive
        };
    }

    public async Task<(bool Success, string? Error)> CreateAsync(UserCreateViewModel model)
    {
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            DepartmentId = model.DepartmentId,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, model.Role);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UserEditViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return (false, "Kullanıcı bulunamadı");

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.DepartmentId = model.DepartmentId;
        user.IsActive = model.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        // Update role
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        return (true, null);
    }

    public async Task ToggleActiveAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
    }

    public async Task<int> GetUserCountAsync()
        => await _context.Users.CountAsync();
}
