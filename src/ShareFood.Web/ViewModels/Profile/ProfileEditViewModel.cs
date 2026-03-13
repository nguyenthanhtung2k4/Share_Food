using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Profile;

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [Display(Name = "Họ và tên")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Họ và tên phải từ 3 đến 120 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Nhận email về công thức mới và mẹo nấu ăn")]
    public bool ReceiveNewsEmails { get; set; }
}
