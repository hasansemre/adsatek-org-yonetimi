namespace OrgYonetimi.Models;

public class GorevDosya
{
    public int Id { get; set; }
    public int GorevItemId { get; set; }
    public int? GorevAsamaId { get; set; }
    public string DosyaAdi { get; set; } = "";
    public string DosyaYolu { get; set; } = "";
    public string DosyaTipi { get; set; } = ""; // image, pdf, office
    public long DosyaBoyutu { get; set; }
    public string YukleyenKullaniciId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GorevItem GorevItem { get; set; } = null!;
    public GorevAsama? GorevAsama { get; set; }
    public AppUser YukleyenKullanici { get; set; } = null!;
}
