namespace OrgYonetimi.Models;

public class Project
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public string Durum { get; set; } = "aktif"; // aktif, tamamlandi, beklemede, iptal
    public DateTime BaslangicTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? BitisTarihi { get; set; }
    public string YoneticiId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Yonetici { get; set; } = null!;
    public ICollection<GorevItem> Gorevler { get; set; } = new List<GorevItem>();
    public ICollection<Zimmet> Zimmetler { get; set; } = new List<Zimmet>();
}
