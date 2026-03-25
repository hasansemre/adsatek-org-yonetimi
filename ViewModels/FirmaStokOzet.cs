namespace OrgYonetimi.ViewModels;

public class FirmaStokOzet
{
    public int? TemsilciFirmaId { get; set; }
    public string FirmaAdi { get; set; } = "Genel";
    public decimal ToplamGiris { get; set; }
    public decimal ToplamCikis { get; set; }
    public decimal MevcutStok => ToplamGiris - ToplamCikis;
}
