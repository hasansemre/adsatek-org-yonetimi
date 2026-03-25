using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrgYonetimi.Models;

namespace OrgYonetimi.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<GorevItem> GorevItems => Set<GorevItem>();
    public DbSet<GorevAsama> GorevAsamalar => Set<GorevAsama>();
    public DbSet<GorevDosya> GorevDosyalar => Set<GorevDosya>();
    public DbSet<Firma> Firmalar => Set<Firma>();
    public DbSet<StokKarti> StokKartlari => Set<StokKarti>();
    public DbSet<Tedarik> Tedarikler => Set<Tedarik>();
    public DbSet<StokHareket> StokHareketler => Set<StokHareket>();
    public DbSet<GorevStokKullanim> GorevStokKullanimlari => Set<GorevStokKullanim>();
    public DbSet<Zimmet> Zimmetler => Set<Zimmet>();
    public DbSet<Satis> Satislar => Set<Satis>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Department self-referencing
        builder.Entity<Department>()
            .HasOne(d => d.UstDepartman)
            .WithMany(d => d.AltDepartmanlar)
            .HasForeignKey(d => d.UstDepartmanId)
            .OnDelete(DeleteBehavior.Restrict);

        // AppUser -> Department
        builder.Entity<AppUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Kullanicilar)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Project -> Yonetici
        builder.Entity<Project>()
            .HasOne(p => p.Yonetici)
            .WithMany(u => u.YonetilenProjeler)
            .HasForeignKey(p => p.YoneticiId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevItem -> Project
        builder.Entity<GorevItem>()
            .HasOne(g => g.Project)
            .WithMany(p => p.Gorevler)
            .HasForeignKey(g => g.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // GorevItem -> AtananKullanici
        builder.Entity<GorevItem>()
            .HasOne(g => g.AtananKullanici)
            .WithMany(u => u.AtananGorevler)
            .HasForeignKey(g => g.AtananKullaniciId)
            .OnDelete(DeleteBehavior.SetNull);

        // GorevItem -> Olusturan
        builder.Entity<GorevItem>()
            .HasOne(g => g.Olusturan)
            .WithMany()
            .HasForeignKey(g => g.OlusturanId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevAsama -> GorevItem
        builder.Entity<GorevAsama>()
            .HasOne(a => a.GorevItem)
            .WithMany(g => g.Asamalar)
            .HasForeignKey(a => a.GorevItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // GorevAsama -> YapanKullanici
        builder.Entity<GorevAsama>()
            .HasOne(a => a.YapanKullanici)
            .WithMany()
            .HasForeignKey(a => a.YapanKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevDosya -> GorevItem
        builder.Entity<GorevDosya>()
            .HasOne(d => d.GorevItem)
            .WithMany(g => g.Dosyalar)
            .HasForeignKey(d => d.GorevItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // GorevDosya -> GorevAsama (nullable)
        builder.Entity<GorevDosya>()
            .HasOne(d => d.GorevAsama)
            .WithMany(a => a.Dosyalar)
            .HasForeignKey(d => d.GorevAsamaId)
            .OnDelete(DeleteBehavior.SetNull);

        // GorevDosya -> YukleyenKullanici
        builder.Entity<GorevDosya>()
            .HasOne(d => d.YukleyenKullanici)
            .WithMany()
            .HasForeignKey(d => d.YukleyenKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tedarik -> StokKarti
        builder.Entity<Tedarik>()
            .HasOne(t => t.StokKarti)
            .WithMany(s => s.Tedarikler)
            .HasForeignKey(t => t.StokKartiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tedarik -> Project
        builder.Entity<Tedarik>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tedarik -> TedarikFirma
        builder.Entity<Tedarik>()
            .HasOne(t => t.TedarikFirma)
            .WithMany(f => f.TedarikciTedarikler)
            .HasForeignKey(t => t.TedarikFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tedarik -> TemsilciFirma
        builder.Entity<Tedarik>()
            .HasOne(t => t.TemsilciFirma)
            .WithMany(f => f.TemsilciTedarikler)
            .HasForeignKey(t => t.TemsilciFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tedarik -> Olusturan
        builder.Entity<Tedarik>()
            .HasOne(t => t.Olusturan)
            .WithMany()
            .HasForeignKey(t => t.OlusturanId)
            .OnDelete(DeleteBehavior.Restrict);

        // StokHareket -> StokKarti
        builder.Entity<StokHareket>()
            .HasOne(h => h.StokKarti)
            .WithMany()
            .HasForeignKey(h => h.StokKartiId)
            .OnDelete(DeleteBehavior.Restrict);

        // StokHareket -> TemsilciFirma
        builder.Entity<StokHareket>()
            .HasOne(h => h.TemsilciFirma)
            .WithMany()
            .HasForeignKey(h => h.TemsilciFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // StokHareket -> Tedarik
        builder.Entity<StokHareket>()
            .HasOne(h => h.Tedarik)
            .WithMany()
            .HasForeignKey(h => h.TedarikId)
            .OnDelete(DeleteBehavior.SetNull);

        // StokHareket -> GorevItem
        builder.Entity<StokHareket>()
            .HasOne(h => h.GorevItem)
            .WithMany()
            .HasForeignKey(h => h.GorevItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // StokHareket -> Olusturan
        builder.Entity<StokHareket>()
            .HasOne(h => h.Olusturan)
            .WithMany()
            .HasForeignKey(h => h.OlusturanId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevStokKullanim -> GorevItem
        builder.Entity<GorevStokKullanim>()
            .HasOne(k => k.GorevItem)
            .WithMany(g => g.StokKullanimlari)
            .HasForeignKey(k => k.GorevItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // GorevStokKullanim -> StokKarti
        builder.Entity<GorevStokKullanim>()
            .HasOne(k => k.StokKarti)
            .WithMany()
            .HasForeignKey(k => k.StokKartiId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevStokKullanim -> TemsilciFirma
        builder.Entity<GorevStokKullanim>()
            .HasOne(k => k.TemsilciFirma)
            .WithMany()
            .HasForeignKey(k => k.TemsilciFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // GorevStokKullanim -> Kullanan
        builder.Entity<GorevStokKullanim>()
            .HasOne(k => k.Kullanan)
            .WithMany()
            .HasForeignKey(k => k.KullananId)
            .OnDelete(DeleteBehavior.Restrict);

        // Zimmet -> Project
        builder.Entity<Zimmet>()
            .HasOne(z => z.Project)
            .WithMany(p => p.Zimmetler)
            .HasForeignKey(z => z.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Zimmet -> TeslimAlanKisi
        builder.Entity<Zimmet>()
            .HasOne(z => z.TeslimAlanKisi)
            .WithMany()
            .HasForeignKey(z => z.TeslimAlanKisiId)
            .OnDelete(DeleteBehavior.SetNull);

        // Zimmet -> Olusturan
        builder.Entity<Zimmet>()
            .HasOne(z => z.Olusturan)
            .WithMany()
            .HasForeignKey(z => z.OlusturanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satis -> StokKarti
        builder.Entity<Satis>()
            .HasOne(s => s.StokKarti)
            .WithMany(sk => sk.Satislar)
            .HasForeignKey(s => s.StokKartiId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satis -> MusteriFirma
        builder.Entity<Satis>()
            .HasOne(s => s.MusteriFirma)
            .WithMany(f => f.MusteriSatislar)
            .HasForeignKey(s => s.MusteriFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satis -> TemsilciFirma
        builder.Entity<Satis>()
            .HasOne(s => s.TemsilciFirma)
            .WithMany(f => f.TemsilciSatislar)
            .HasForeignKey(s => s.TemsilciFirmaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satis -> Project
        builder.Entity<Satis>()
            .HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Satis -> Olusturan
        builder.Entity<Satis>()
            .HasOne(s => s.Olusturan)
            .WithMany()
            .HasForeignKey(s => s.OlusturanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision
        builder.Entity<Tedarik>().Property(t => t.Miktar).HasPrecision(18, 4);
        builder.Entity<Tedarik>().Property(t => t.BirimFiyat).HasPrecision(18, 4);
        builder.Entity<Tedarik>().Property(t => t.ToplamTutar).HasPrecision(18, 4);
        builder.Entity<StokKarti>().Property(s => s.MinStok).HasPrecision(18, 4);
        builder.Entity<StokKarti>().Property(s => s.BirimFiyat).HasPrecision(18, 4);
        builder.Entity<StokHareket>().Property(h => h.Miktar).HasPrecision(18, 4);
        builder.Entity<GorevStokKullanim>().Property(k => k.Miktar).HasPrecision(18, 4);
        builder.Entity<Satis>().Property(s => s.BirimFiyat).HasPrecision(18, 4);
        builder.Entity<Satis>().Property(s => s.ToplamTutar).HasPrecision(18, 4);
    }
}
