namespace OrgYonetimi.Models;

public class Department
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int? UstDepartmanId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department? UstDepartman { get; set; }
    public ICollection<Department> AltDepartmanlar { get; set; } = new List<Department>();
    public ICollection<AppUser> Kullanicilar { get; set; } = new List<AppUser>();
}
