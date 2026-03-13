using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Recipes;

public class CommentInputViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận.")]
    [Display(Name = "Nội dung bình luận")]
    [StringLength(1000, MinimumLength = 2, ErrorMessage = "Bình luận phải từ 2 đến 1000 ký tự.")]
    public string Content { get; set; } = string.Empty;
}
