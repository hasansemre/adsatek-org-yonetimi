using OrgYonetimi.Models;

namespace OrgYonetimi.ViewModels;

public class DashboardViewModel
{
    public int ToplamKullanici { get; set; }
    public int ToplamProje { get; set; }
    public int AktifGorev { get; set; }
    public int GecikmiGorev { get; set; }
    public int ToplamDepartman { get; set; }
    public int TamamlananGorev { get; set; }
    public List<GorevItem> SonGorevler { get; set; } = new();
    public List<Project> SonProjeler { get; set; } = new();
    public int ToplamStokKarti { get; set; }
    public int DusukStokUrun { get; set; }
    public List<StokDurumItem> StokDurumlari { get; set; } = new();
    public List<TedarikciOzet> TedarikciOzetleri { get; set; } = new();
    public List<TemsilciOzet> TemsilciOzetleri { get; set; } = new();
    public decimal ToplamSatisTutari { get; set; }
    public int ToplamSatisAdedi { get; set; }
    public decimal TeslimEdilenSatis { get; set; }
    public decimal BekleyenSatis { get; set; }
    public List<SatisOzet> SatisOzetleri { get; set; } = new();
}

public class StokDurumItem
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string Birim { get; set; } = "";
    public int MevcutStok { get; set; }
    public int MinStok { get; set; }
    public bool DusukStok => MevcutStok <= MinStok;
}

public class TedarikciOzet
{
    public int FirmaId { get; set; }
    public string FirmaAdi { get; set; } = "";
    public int ToplamSiparis { get; set; }
    public int TeslimAlinan { get; set; }
    public int Bekleyen { get; set; }
    public decimal ToplamHarcama { get; set; } // Teslim alınan TL tutarı
    public decimal BekleyenTutar { get; set; } // Bekleyen/onaylanan TL tutarı
}

public class TemsilciOzet
{
    public int FirmaId { get; set; }
    public string FirmaAdi { get; set; } = "";
    public int ToplamUrun { get; set; } // Kaç farklı ürün stoğu var
    public int ToplamStokAdedi { get; set; } // Toplam stok adedi
    public decimal ToplamHarcama { get; set; } // Temsilci firma üzerinden yapılan toplam harcama TL
    public List<TemsilciStokDetay> StokDetaylari { get; set; } = new();
}

public class TemsilciStokDetay
{
    public string UrunAdi { get; set; } = "";
    public int Adet { get; set; }
    public string Birim { get; set; } = "";
}

public class SatisOzet
{
    public string UrunAdi { get; set; } = "";
    public string Tip { get; set; } = "urun";
    public string MusteriFirmaAdi { get; set; } = "";
    public int Miktar { get; set; }
    public decimal Tutar { get; set; }
    public string Durum { get; set; } = "";
    public DateTime Tarih { get; set; }
}
