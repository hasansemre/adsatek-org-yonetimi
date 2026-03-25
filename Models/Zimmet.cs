namespace OrgYonetimi.Models;

public class Zimmet
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Ad { get; set; } = ""; // Zimmet adı
    public string? Aciklama { get; set; }
    public string? SeriNo { get; set; } // Seri/Takip numarası
    public int Adet { get; set; } = 1;
    public string? TeslimAlanKisiId { get; set; } // Zimmeti alan kişi
    public DateTime TeslimTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? IadeTarihi { get; set; }
    public string Durum { get; set; } = "teslim_edildi"; // teslim_edildi, iade_edildi, kayip, hasarli
    public string OlusturanId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public AppUser? TeslimAlanKisi { get; set; }
    public AppUser Olusturan { get; set; } = null!;
}
