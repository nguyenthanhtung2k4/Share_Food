using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Admin;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
    [Display(Name = "Tên danh mục")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2 đến 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }
}
