namespace OrgYonetimi.Models;

public class GorevStokKullanim
{
    public int Id { get; set; }
    public int GorevItemId { get; set; }
    public int StokKartiId { get; set; }
    public int? TemsilciFirmaId { get; set; } // Hangi firmanın stoğundan kullanıldı
    public decimal Miktar { get; set; }
    public string? Aciklama { get; set; }
    public string KullananId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GorevItem GorevItem { get; set; } = null!;
    public StokKarti StokKarti { get; set; } = null!;
    public Firma? TemsilciFirma { get; set; }
    public AppUser Kullanan { get; set; } = null!;
}
