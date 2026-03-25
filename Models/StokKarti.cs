namespace OrgYonetimi.Models;

public class StokKarti
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Kod { get; set; } // SKU / Stok kodu
    public string? Aciklama { get; set; }
    public string Birim { get; set; } = "adet"; // adet, kg, metre, litre, paket, kutu, set
    public string? Kategori { get; set; }
    public string Tip { get; set; } = "urun"; // urun, hizmet
    public decimal MinStok { get; set; } = 0;
    public decimal BirimFiyat { get; set; } = 0; // Varsayılan satış fiyatı
    public string ParaBirimi { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Tedarik> Tedarikler { get; set; } = new();
    public List<Satis> Satislar { get; set; } = new();
}
