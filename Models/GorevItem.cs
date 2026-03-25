namespace OrgYonetimi.Models;

public class GorevItem
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string? Aciklama { get; set; }
    public string Durum { get; set; } = "bekliyor"; // bekliyor, devam, tamamlandi, gecikti
    public string Oncelik { get; set; } = "normal"; // kritik, yuksek, normal, dusuk
    public DateTime? SonTarih { get; set; }
    public DateTime? TamamlanmaTarihi { get; set; }
    public int? ProjectId { get; set; }
    public string? AtananKullaniciId { get; set; }
    public string OlusturanId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    public AppUser? AtananKullanici { get; set; }
    public AppUser Olusturan { get; set; } = null!;
    public List<GorevAsama> Asamalar { get; set; } = new();
    public List<GorevDosya> Dosyalar { get; set; } = new();
    public List<GorevStokKullanim> StokKullanimlari { get; set; } = new();
}
