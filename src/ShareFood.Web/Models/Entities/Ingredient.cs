using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
//  Nguyên liêu
public class Ingredient : BaseEntity
{
    [Required(ErrorMessage = "Vui lòng nhập tên nguyên liệu.")]
    [Display(Name = "Tên nguyên liệu")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Tên nguyên liệu phải từ 2 đến 120 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    [StringLength(300, ErrorMessage = "Mô tả không được vượt quá 300 ký tự.")]
    public string? Description { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
