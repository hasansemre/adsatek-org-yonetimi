namespace OrgYonetimi.Models;

public class Satis
{
    public int Id { get; set; }
    public int StokKartiId { get; set; }
    public int MusteriFirmaId { get; set; } // Satış yapılan firma
    public int? TemsilciFirmaId { get; set; } // Satışı yapan temsilci firma
    public int? ProjectId { get; set; }
    public int Miktar { get; set; }
    public string Birim { get; set; } = "adet";
    public decimal BirimFiyat { get; set; }
    public decimal ToplamTutar { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public string? FaturaNo { get; set; }
    public DateTime Tarih { get; set; } = DateTime.UtcNow;
    public string Durum { get; set; } = "beklemede"; // beklemede, onaylandi, teslim_edildi, iptal
    public string? Aciklama { get; set; }
    public string OlusturanId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StokKarti StokKarti { get; set; } = null!;
    public Firma MusteriFirma { get; set; } = null!;
    public Firma? TemsilciFirma { get; set; }
    public Project? Project { get; set; }
    public AppUser Olusturan { get; set; } = null!;
}
