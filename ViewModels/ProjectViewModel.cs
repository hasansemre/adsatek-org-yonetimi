using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class ProjectViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Proje adı gereklidir")]
    public string Ad { get; set; } = "";

    public string? Aciklama { get; set; }
    public string Durum { get; set; } = "aktif";

    [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
    [DataType(DataType.Date)]
    public DateTime BaslangicTarihi { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? BitisTarihi { get; set; }

    [Required(ErrorMessage = "Proje yöneticisi seçiniz")]
    public string YoneticiId { get; set; } = "";
}
