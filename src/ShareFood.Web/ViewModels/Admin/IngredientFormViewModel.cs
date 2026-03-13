using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Admin;

public class IngredientFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên nguyên liệu.")]
    [Display(Name = "Tên nguyên liệu")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Tên nguyên liệu phải từ 2 đến 120 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    [StringLength(300, ErrorMessage = "Mô tả không được vượt quá 300 ký tự.")]
    public string? Description { get; set; }
}
