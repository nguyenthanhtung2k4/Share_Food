using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.Models.Entities;
// Đơn vị đo lường nguyên liệu (ví dụ: gram, cup, tablespoon)
public class Unit : BaseEntity
{
    [Required(ErrorMessage = "Vui lòng nhập tên đơn vị.")]
    [Display(Name = "Tên đơn vị")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Tên đơn vị phải từ 1 đến 80 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập ký hiệu.")]
    [Display(Name = "Ký hiệu")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Ký hiệu không được vượt quá 20 ký tự.")]
    public string Symbol { get; set; } = string.Empty;

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
