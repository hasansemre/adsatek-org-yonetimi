using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class FirmaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Firma adı zorunludur")]
    [Display(Name = "Firma Adı")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "Firma tipi zorunludur")]
    [Display(Name = "Firma Tipi")]
    public string Tip { get; set; } = "tedarikci";

    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [Display(Name = "Yetkili Kişi")]
    public string? YetkiliKisi { get; set; }

    [Display(Name = "Vergi No")]
    public string? VergiNo { get; set; }

    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}
