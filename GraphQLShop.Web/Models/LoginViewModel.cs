using System.ComponentModel.DataAnnotations;

namespace GraphQLShop.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}