using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Models;
using OrgYonetimi.Services;

// Npgsql: DateTime Kind uyumsuzlugunu coz
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Railway PORT desteği - builder aşamasında ayarla
var port = Environment.GetEnvironmentVariable("PORT") ?? "5123";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// MVC
builder.Services.AddControllersWithViews();

// PostgreSQL + EF Core
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL");
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway PostgreSQL URL formatını Npgsql formatına dönüştür
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    catch
    {
        // URL zaten Npgsql formatında olabilir
        connectionString = databaseUrl;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}
Console.WriteLine($"[DB] Connection target: {(string.IsNullOrEmpty(databaseUrl) ? "local appsettings" : "DATABASE_URL env")}");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Services DI
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IGorevService, GorevService>();
builder.Services.AddScoped<IFirmaService, FirmaService>();
builder.Services.AddScoped<IStokService, StokService>();
builder.Services.AddScoped<ITedarikService, TedarikService>();
builder.Services.AddScoped<ISatisService, SatisService>();

var app = builder.Build();

// Seed data
await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
