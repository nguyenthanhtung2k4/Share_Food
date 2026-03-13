using System.ComponentModel.DataAnnotations;

namespace ShareFood.Web.ViewModels.Account;

public class VerifyOtpViewModel
{
    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
    [Display(Name = "Mã OTP")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số.")]
    public string Code { get; set; } = string.Empty;
}
