using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Data;
using OrgYonetimi.Services;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly IGorevService _gorevService;
    private readonly IStokService _stokService;
    private readonly ISatisService _satisService;

    public DashboardController(AppDbContext context, IGorevService gorevService, IStokService stokService, ISatisService satisService)
    {
        _context = context;
        _gorevService = gorevService;
        _stokService = stokService;
        _satisService = satisService;
    }

    // Kullanıcının departmanındaki kullanıcı ID'lerini döner
    private async Task<List<string>?> GetDepartmanKullanicilariAsync()
    {
        if (User.IsInRole("Admin")) return null; // Admin her şeyi görür

        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
        if (currentUser?.DepartmentId == null) return new List<string> { currentUser?.Id ?? "" };

        var departmanKullanicilari = await _context.Users
            .Where(u => u.DepartmentId == currentUser.DepartmentId)
            .Select(u => u.Id)
            .ToListAsync();

        return departmanKullanicilari;
    }

    public async Task<IActionResult> Index()
    {
        await _gorevService.CheckOverdueAsync();

        var isAdmin = User.IsInRole("Admin");
        var isCalisan = User.IsInRole("Calisan");
        var deptUserIds = await GetDepartmanKullanicilariAsync();

        // Görev sorguları - departman filtreli
        var gorevQuery = _context.GorevItems.AsQueryable();
        var projeQuery = _context.Projects.AsQueryable();

        if (deptUserIds != null)
        {
            gorevQuery = gorevQuery.Where(g =>
                deptUserIds.Contains(g.AtananKullaniciId ?? "") ||
                deptUserIds.Contains(g.OlusturanId));
            projeQuery = projeQuery.Where(p =>
                deptUserIds.Contains(p.YoneticiId ?? ""));
        }

        var model = new DashboardViewModel
        {
            ToplamKullanici = isAdmin
                ? await _context.Users.CountAsync()
                : (deptUserIds?.Count ?? 0),
            ToplamProje = await projeQuery.CountAsync(),
            AktifGorev = await gorevQuery.CountAsync(g => g.Durum == "bekliyor" || g.Durum == "devam"),
            GecikmiGorev = await gorevQuery.CountAsync(g => g.Durum == "gecikti"),
            ToplamDepartman = await _context.Departments.CountAsync(),
            TamamlananGorev = await gorevQuery.CountAsync(g => g.Durum == "tamamlandi"),
            SonGorevler = await gorevQuery
                .Include(g => g.AtananKullanici)
                .Include(g => g.Project)
                .OrderByDescending(g => g.CreatedAt)
                .Take(5)
                .ToListAsync(),
            SonProjeler = await projeQuery
                .Include(p => p.Yonetici)
                .Include(p => p.Gorevler)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync()
        };

        // Stok ve Tedarik bilgileri - sadece Admin ve Yönetici
        if (!isCalisan)
        {
            // Tedarik sorgusu - Yönetici ise departman filtreli
            var tedarikQuery = _context.Tedarikler.AsQueryable();
            var stokHareketQuery = _context.StokHareketler.AsQueryable();

            if (deptUserIds != null)
            {
                tedarikQuery = tedarikQuery.Where(t => deptUserIds.Contains(t.OlusturanId));
                stokHareketQuery = stokHareketQuery.Where(h => deptUserIds.Contains(h.OlusturanId));
            }

            // Stok kartları - departman filtreli stok hesaplama
            var stokKartlari = await _stokService.GetAllAsync();
            var stokDurumlari = new List<StokDurumItem>();
            int dusukStok = 0;

            foreach (var sk in stokKartlari.Where(s => s.Tip == "urun"))
            {
                int miktar;
                if (deptUserIds != null)
                {
                    miktar = (int)await stokHareketQuery
                        .Where(h => h.StokKartiId == sk.Id)
                        .SumAsync(h => h.Miktar);
                }
                else
                {
                    miktar = (int)await _stokService.GetToplamStokAsync(sk.Id);
                }

                stokDurumlari.Add(new StokDurumItem
                {
                    Id = sk.Id,
                    Ad = sk.Ad,
                    Birim = sk.Birim,
                    MevcutStok = miktar,
                    MinStok = (int)sk.MinStok
                });
                if (miktar <= (int)sk.MinStok) dusukStok++;
            }

            model.ToplamStokKarti = stokKartlari.Count(s => s.Tip == "urun");
            model.DusukStokUrun = dusukStok;
            model.StokDurumlari = stokDurumlari.OrderByDescending(s => s.DusukStok).ThenBy(s => s.Ad).ToList();

            // Tedarikçi firma özetleri - departman filtreli
            model.TedarikciOzetleri = await tedarikQuery
                .Include(t => t.TedarikFirma)
                .GroupBy(t => new { t.TedarikFirmaId, t.TedarikFirma.Ad })
                .Select(g => new TedarikciOzet
                {
                    FirmaId = g.Key.TedarikFirmaId,
                    FirmaAdi = g.Key.Ad,
                    ToplamSiparis = g.Count(),
                    TeslimAlinan = g.Count(t => t.Durum == "teslim_alindi"),
                    Bekleyen = g.Count(t => t.Durum == "beklemede" || t.Durum == "onaylandi"),
                    ToplamHarcama = g.Where(t => t.Durum == "teslim_alindi").Sum(t => t.ToplamTutar),
                    BekleyenTutar = g.Where(t => t.Durum == "beklemede" || t.Durum == "onaylandi").Sum(t => t.ToplamTutar)
                })
                .OrderByDescending(o => o.ToplamHarcama)
                .ToListAsync();

            // Temsilci firma özetleri - departman filtreli
            // Yönetici: sadece kendi departmanının tedariklerindeki temsilci firmaları görsün
            IQueryable<int?> temsilciFirmaIds;
            if (deptUserIds != null)
            {
                temsilciFirmaIds = tedarikQuery
                    .Where(t => t.TemsilciFirmaId != null)
                    .Select(t => t.TemsilciFirmaId)
                    .Distinct();
            }
            else
            {
                temsilciFirmaIds = _context.Firmalar
                    .Where(f => f.Tip == "temsilci" && f.IsActive)
                    .Select(f => (int?)f.Id);
            }

            var temsilciFirmalar = await _context.Firmalar
                .Where(f => f.Tip == "temsilci" && f.IsActive && temsilciFirmaIds.Contains(f.Id))
                .ToListAsync();

            var temsilciOzetleri = new List<TemsilciOzet>();
            foreach (var firma in temsilciFirmalar)
            {
                var harcama = await tedarikQuery
                    .Where(t => t.TemsilciFirmaId == firma.Id && t.Durum == "teslim_alindi")
                    .SumAsync(t => t.ToplamTutar);

                var stoklar = await stokHareketQuery
                    .Where(h => h.TemsilciFirmaId == firma.Id)
                    .Include(h => h.StokKarti)
                    .GroupBy(h => new { h.StokKartiId, h.StokKarti.Ad, h.StokKarti.Birim })
                    .Select(g => new
                    {
                        UrunAdi = g.Key.Ad,
                        Birim = g.Key.Birim,
                        Adet = (int)g.Sum(h => h.Miktar)
                    })
                    .Where(s => s.Adet > 0)
                    .ToListAsync();

                temsilciOzetleri.Add(new TemsilciOzet
                {
                    FirmaId = firma.Id,
                    FirmaAdi = firma.Ad,
                    ToplamUrun = stoklar.Count,
                    ToplamStokAdedi = stoklar.Sum(s => s.Adet),
                    ToplamHarcama = harcama,
                    StokDetaylari = stoklar.Select(s => new TemsilciStokDetay
                    {
                        UrunAdi = s.UrunAdi,
                        Adet = s.Adet,
                        Birim = s.Birim
                    }).ToList()
                });
            }
            model.TemsilciOzetleri = temsilciOzetleri.OrderByDescending(t => t.ToplamHarcama).ToList();

            // Satış verileri
            var satisQuery = _context.Satislar.AsQueryable();
            if (deptUserIds != null)
            {
                satisQuery = satisQuery.Where(s => deptUserIds.Contains(s.OlusturanId));
            }

            model.ToplamSatisAdedi = await satisQuery.CountAsync(s => s.Durum != "iptal");
            model.ToplamSatisTutari = await satisQuery.Where(s => s.Durum != "iptal").SumAsync(s => s.ToplamTutar);
            model.TeslimEdilenSatis = await satisQuery.Where(s => s.Durum == "teslim_edildi").SumAsync(s => s.ToplamTutar);
            model.BekleyenSatis = await satisQuery.Where(s => s.Durum == "beklemede" || s.Durum == "onaylandi").SumAsync(s => s.ToplamTutar);

            model.SatisOzetleri = await satisQuery
                .Include(s => s.StokKarti)
                .Include(s => s.MusteriFirma)
                .Where(s => s.Durum != "iptal")
                .OrderByDescending(s => s.Tarih)
                .Take(10)
                .Select(s => new SatisOzet
                {
                    UrunAdi = s.StokKarti.Ad,
                    Tip = s.StokKarti.Tip,
                    MusteriFirmaAdi = s.MusteriFirma.Ad,
                    Miktar = s.Miktar,
                    Tutar = s.ToplamTutar,
                    Durum = s.Durum,
                    Tarih = s.Tarih
                })
                .ToListAsync();
        }

        return View(model);
    }
}
