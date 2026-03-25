using System.ComponentModel.DataAnnotations;

namespace OrgYonetimi.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
