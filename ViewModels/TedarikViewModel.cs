using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class TedarikViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün/Hizmet seçimi zorunludur")]
    [Display(Name = "Ürün/Hizmet")]
    public int StokKartiId { get; set; }

    [Display(Name = "Proje")]
    public int? ProjectId { get; set; }

    [Required(ErrorMessage = "Tedarikçi firma seçimi zorunludur")]
    [Display(Name = "Tedarikçi Firma")]
    public int TedarikFirmaId { get; set; }

    [Display(Name = "Temsilci Firma")]
    public int? TemsilciFirmaId { get; set; }

    [Required]
    [Display(Name = "Miktar")]
    public decimal Miktar { get; set; }

    [Required]
    [Display(Name = "Birim")]
    public string Birim { get; set; } = "adet";

    [Required]
    [Display(Name = "Birim Fiyat")]
    public decimal BirimFiyat { get; set; }

    [Display(Name = "Para Birimi")]
    public string ParaBirimi { get; set; } = "TRY";

    [Display(Name = "Fatura No")]
    public string? FaturaNo { get; set; }

    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Now;

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}
