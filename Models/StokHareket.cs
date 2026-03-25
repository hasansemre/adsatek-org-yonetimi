namespace OrgYonetimi.Models;

public class StokHareket
{
    public int Id { get; set; }
    public int StokKartiId { get; set; }
    public int? TemsilciFirmaId { get; set; } // Hangi temsilci firmanın stoğu
    public int? TedarikId { get; set; } // Tedarikten gelen giriş
    public int? GorevItemId { get; set; } // Görevde kullanılan çıkış
    public decimal Miktar { get; set; } // + giriş, - çıkış
    public string HareketTipi { get; set; } = "giris"; // giris, cikis, transfer
    public string? Aciklama { get; set; }
    public string OlusturanId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StokKarti StokKarti { get; set; } = null!;
    public Firma? TemsilciFirma { get; set; }
    public Tedarik? Tedarik { get; set; }
    public GorevItem? GorevItem { get; set; }
    public AppUser Olusturan { get; set; } = null!;
}
