using Microsoft.AspNetCore.Identity;

namespace OrgYonetimi.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = "";
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department? Department { get; set; }
    public ICollection<GorevItem> AtananGorevler { get; set; } = new List<GorevItem>();
    public ICollection<Project> YonetilenProjeler { get; set; } = new List<Project>();
}
