using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class StokKartiViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün/Hizmet adı zorunludur")]
    [Display(Name = "Ürün/Hizmet Adı")]
    public string Ad { get; set; } = "";

    [Display(Name = "Stok Kodu")]
    public string? Kod { get; set; }

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Required]
    [Display(Name = "Birim")]
    public string Birim { get; set; } = "adet";

    [Display(Name = "Kategori")]
    public string? Kategori { get; set; }

    [Required]
    [Display(Name = "Tip")]
    public string Tip { get; set; } = "urun";

    [Display(Name = "Minimum Stok")]
    public decimal MinStok { get; set; } = 0;

    [Display(Name = "Birim Fiyat")]
    public decimal BirimFiyat { get; set; } = 0;

    [Display(Name = "Para Birimi")]
    public string ParaBirimi { get; set; } = "TRY";
}
