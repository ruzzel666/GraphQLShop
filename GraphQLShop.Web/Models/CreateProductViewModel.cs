using System.ComponentModel.DataAnnotations;

namespace GraphQLShop.Web.Models;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "Пожалуйста, введите название")]
    [Display(Name = "Название товара")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
    [Display(Name = "Цена")]
    public double Price { get; set; }

    [Required(ErrorMessage = "Укажите категорию")]
    [Display(Name = "Название категории")]
    public string CategoryName { get; set; } = string.Empty;
}