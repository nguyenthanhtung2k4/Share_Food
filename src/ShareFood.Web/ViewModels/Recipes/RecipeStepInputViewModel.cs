using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeStepInputViewModel
{
    [Display(Name = "Bước nấu")]
    [Range(1, 50, ErrorMessage = "Số thứ tự bước nấu không hợp lệ.")]
    public int StepNumber { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung bước nấu.")]
    [Display(Name = "Nội dung")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Nội dung bước nấu phải từ 5 đến 2000 ký tự.")]
    public string Instruction { get; set; } = string.Empty;
}
