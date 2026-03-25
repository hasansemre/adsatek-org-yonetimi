namespace OrgYonetimi.Models;

public class Firma
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Tip { get; set; } = "tedarikci"; // temsilci, tedarikci
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    public string? YetkiliKisi { get; set; }
    public string? VergiNo { get; set; }
    public string? Aciklama { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Tedarik> TedarikciTedarikler { get; set; } = new();
    public List<Tedarik> TemsilciTedarikler { get; set; } = new();
    public List<Satis> MusteriSatislar { get; set; } = new();
    public List<Satis> TemsilciSatislar { get; set; } = new();
}
