using Microsoft.AspNetCore.Identity;
using OrgYonetimi.Models;

namespace OrgYonetimi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.EnsureCreatedAsync();

        // Seed roles
        string[] roles = { "Admin", "Yonetici", "Calisan" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        if (await userManager.FindByEmailAsync("admin@org.com") == null)
        {
            var admin = new AppUser
            {
                UserName = "admin@org.com",
                Email = "admin@org.com",
                FullName = "Sistem Yöneticisi",
                IsActive = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed departments
        if (!context.Departments.Any())
        {
            var yonetim = new Department { Ad = "Yönetim", Aciklama = "Üst yönetim birimi" };
            context.Departments.Add(yonetim);
            await context.SaveChangesAsync();

            context.Departments.AddRange(
                new Department { Ad = "Yazılım Geliştirme", Aciklama = "Yazılım ekibi", UstDepartmanId = yonetim.Id },
                new Department { Ad = "İnsan Kaynakları", Aciklama = "İK birimi", UstDepartmanId = yonetim.Id },
                new Department { Ad = "Muhasebe", Aciklama = "Mali işler birimi", UstDepartmanId = yonetim.Id }
            );
            await context.SaveChangesAsync();
        }
    }
}
