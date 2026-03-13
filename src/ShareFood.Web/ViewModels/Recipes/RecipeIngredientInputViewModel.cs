using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeIngredientInputViewModel
{
    [Display(Name = "Nguyên liệu")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nguyên liệu.")]
    public int IngredientId { get; set; }

    [Display(Name = "Số lượng")]
    [Range(typeof(decimal), "0.1", "99999", ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Đơn vị")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị.")]
    public int UnitId { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
    public string? Note { get; set; }
}
