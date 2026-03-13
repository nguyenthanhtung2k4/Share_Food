using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using ShareFood.Web.Helpers;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.Services;
using ShareFood.Web.ViewModels.Account;
using ShareFood.Web.ViewModels.Common;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Route("tai-khoan")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [AllowAnonymous]
    [HttpGet("dang-ky")]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost("dang-ky")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            JoinedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, RoleNames.User);
        await SendConfirmationEmailAsync(user);

        TempData.SetSuccess("Đăng ký thành công", "Tài khoản đã được tạo. Vui lòng kiểm tra email để xác nhận.");
        return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
    }

    [AllowAnonymous]
    [HttpGet("dang-ky-thanh-cong")]
    public IActionResult RegisterConfirmation(string? email)
    {
        return View(new StatusPageViewModel
        {
            Title = "Kiểm tra hộp thư của bạn",
            Message = $"Chúng tôi đã gửi email xác nhận đến {email ?? "địa chỉ email của bạn"}. Hãy mở thư và bấm vào liên kết để kích hoạt tài khoản.",
            IsSuccess = true
        });
    }

    [AllowAnonymous]
    [HttpGet("dang-nhap")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("dang-nhap")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa xác nhận email.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "Client" });
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do đăng nhập sai quá nhiều lần.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
        return View(model);
    }

    [Authorize]
    [HttpPost("dang-xuat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData.SetInfo("Đăng xuất thành công", "Bạn đã đăng xuất khỏi hệ thống.");
        return RedirectToAction("Index", "Home", new { area = "Client" });
    }

    [AllowAnonymous]
    [HttpGet("xac-nhan-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View("StatusPage", new StatusPageViewModel
            {
                Title = "Xác nhận thất bại",
                Message = "Liên kết xác nhận không hợp lệ hoặc đã bị thiếu dữ liệu.",
                IsSuccess = false
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return View("StatusPage", new StatusPageViewModel
            {
                Title = "Không tìm thấy tài khoản",
                Message = "Tài khoản cần xác nhận không tồn tại.",
                IsSuccess = false
            });
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return View("StatusPage", new StatusPageViewModel
        {
            Title = result.Succeeded ? "Xác nhận thành công" : "Xác nhận thất bại",
            Message = result.Succeeded
                ? "Email đã được xác nhận. Bây giờ bạn có thể đăng nhập và chia sẻ công thức yêu thích."
                : "Liên kết xác nhận không còn hiệu lực hoặc đã được sử dụng trước đó.",
            IsSuccess = result.Succeeded
        });
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new { area = "Client", userId = user.Id, token = encodedToken },
            Request.Scheme);

        var content = $"""
                       <div style="font-family:Segoe UI,Arial,sans-serif;line-height:1.6;color:#3d2c25">
                           <h2 style="margin-bottom:8px;">Xác nhận tài khoản Share Food</h2>
                           <p>Xin chào <strong>{user.FullName}</strong>,</p>
                           <p>Cảm ơn bạn đã tham gia cộng đồng chia sẻ công thức nấu ăn. Hãy bấm vào nút bên dưới để xác nhận email.</p>
                           <p>
                               <a href="{confirmationUrl}" style="background:#c96b3b;color:#fff;padding:12px 18px;text-decoration:none;border-radius:10px;display:inline-block;">
                                   Xác nhận email
                               </a>
                           </p>
                           <p>Nếu nút không hoạt động, bạn có thể mở liên kết sau:</p>
                           <p><a href="{confirmationUrl}">{confirmationUrl}</a></p>
                       </div>
                       """;

        await _emailService.SendEmailAsync(user.Email!, user.FullName, "Xác nhận tài khoản Share Food", content);
    }
}
