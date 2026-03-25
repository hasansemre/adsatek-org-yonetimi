using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class UserCreateViewModel
{
    [Required(ErrorMessage = "Ad soyad gereklidir")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gereklidir")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Rol seçiniz")]
    public string Role { get; set; } = "Calisan";

    public int? DepartmentId { get; set; }
}

public class UserEditViewModel
{
    public string Id { get; set; } = "";

    [Required(ErrorMessage = "Ad soyad gereklidir")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Rol seçiniz")]
    public string Role { get; set; } = "";

    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; }
}

public class UserListItemViewModel
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? DepartmentAd { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
