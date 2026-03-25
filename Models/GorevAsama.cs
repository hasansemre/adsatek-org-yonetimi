namespace OrgYonetimi.Models;

public class GorevAsama
{
    public int Id { get; set; }
    public int GorevItemId { get; set; }
    public string Asama { get; set; } = ""; // isleme_alindi, tamamlandi, reddedildi
    public string? Not { get; set; }
    public string YapanKullaniciId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public GorevItem GorevItem { get; set; } = null!;
    public AppUser YapanKullanici { get; set; } = null!;
    public List<GorevDosya> Dosyalar { get; set; } = new();
}
