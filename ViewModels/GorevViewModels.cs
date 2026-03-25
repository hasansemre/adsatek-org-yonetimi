using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class GorevCreateViewModel
{
    [Required(ErrorMessage = "Görev başlığı gereklidir")]
    public string Baslik { get; set; } = "";

    public string? Aciklama { get; set; }
    public string Oncelik { get; set; } = "normal";

    [DataType(DataType.Date)]
    public DateTime? SonTarih { get; set; }

    public int? ProjectId { get; set; }
    public string? AtananKullaniciId { get; set; }
}

public class GorevFilterModel
{
    public string? Durum { get; set; }
    public string? Oncelik { get; set; }
    public int? ProjectId { get; set; }
    public string? AtananKullaniciId { get; set; }
    public string? KullaniciId { get; set; } // Atanan VEYA oluşturan (Çalışan filtresi)
    public List<string>? DepartmanKullanicilari { get; set; } // Departman bazlı filtre (Yönetici)
}
