using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class DepartmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Departman adı gereklidir")]
    public string Ad { get; set; } = "";

    public string? Aciklama { get; set; }
    public int? UstDepartmanId { get; set; }
}
