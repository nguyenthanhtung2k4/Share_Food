using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService)
    {
        _dbContext = dbContext;
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
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (existingUser is not null)
        {
            if (!existingUser.EmailConfirmed)
            {
                await CreateAndSendOtpAsync(existingUser, cancellationToken);
                TempData.SetInfo("Mã OTP đã được gửi lại", "Email này đã đăng ký nhưng chưa xác nhận. Vui lòng kiểm tra hộp thư để lấy mã OTP.");
                return RedirectToAction(nameof(VerifyOtp), new { email = existingUser.Email });
            }

            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng. Vui lòng chọn email khác.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName,
            ReceiveNewsEmails = model.ReceiveNewsEmails,
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
        await CreateAndSendOtpAsync(user, cancellationToken);

        TempData.SetSuccess("Đăng ký thành công", "Tài khoản đã được tạo. Vui lòng nhập mã OTP đã gửi về email để kích hoạt.");
        return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
    }

    [AllowAnonymous]
    [HttpGet("dang-ky-thanh-cong")]
    public IActionResult RegisterConfirmation(string? email)
    {
        return View(new StatusPageViewModel
        {
            Title = "Kiểm tra mã OTP trong hộp thư",
            Message = $"Chúng tôi đã gửi mã OTP đến {email ?? "địa chỉ email của bạn"}. Hãy nhập mã gồm 6 chữ số để kích hoạt tài khoản.",
            IsSuccess = true
        });
    }

    [AllowAnonymous]
    [HttpGet("xac-nhan-otp")]
    public IActionResult VerifyOtp(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(Register));
        }

        return View(new VerifyOtpViewModel
        {
            Email = email
        });
    }

    [AllowAnonymous]
    [HttpPost("xac-nhan-otp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken)
    {
        model.Code = new string(model.Code.Where(char.IsDigit).ToArray());

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản cần xác nhận.");
            return View(model);
        }

        if (user.EmailConfirmed)
        {
            TempData.SetInfo("Email đã xác nhận", "Tài khoản này đã được xác nhận trước đó. Bạn có thể đăng nhập ngay.");
            return RedirectToAction(nameof(Login));
        }

        var otp = await _dbContext.EmailOtps
            .Where(item => item.UserId == user.Id
                           && !item.IsUsed
                           && item.ExpiresAt >= DateTime.UtcNow
                           && item.Code == model.Code)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
        {
            ModelState.AddModelError(nameof(model.Code), "Mã OTP không đúng hoặc đã hết hạn. Vui lòng kiểm tra lại.");
            return View(model);
        }

        otp.IsUsed = true;
        user.EmailConfirmed = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _userManager.UpdateAsync(user);

        await SendWelcomeEmailAsync(user);
        if (user.ReceiveNewsEmails)
        {
            await SendGettingStartedEmailAsync(user, cancellationToken);
        }

        return View("StatusPage", new StatusPageViewModel
        {
            Title = "Xác nhận thành công",
            Message = "Email của bạn đã được xác nhận bằng mã OTP. Bây giờ bạn có thể đăng nhập và bắt đầu chia sẻ công thức.",
            IsSuccess = true
        });
    }

    [AllowAnonymous]
    [HttpPost("gui-lai-otp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(VerifyOtpViewModel model, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            TempData.SetError("Không tìm thấy tài khoản", "Email này chưa được đăng ký trong hệ thống.");
            return RedirectToAction(nameof(Register));
        }

        if (user.EmailConfirmed)
        {
            TempData.SetInfo("Email đã xác nhận", "Tài khoản này đã được xác nhận. Bạn có thể đăng nhập ngay.");
            return RedirectToAction(nameof(Login));
        }

        await CreateAndSendOtpAsync(user, cancellationToken);
        TempData.SetSuccess("Đã gửi lại mã OTP", "Vui lòng kiểm tra hộp thư đến hoặc thư rác để lấy mã mới.");
        return RedirectToAction(nameof(VerifyOtp), new { email = user.Email });
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
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa xác nhận email. Vui lòng nhập mã OTP đã được gửi tới hộp thư.");
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

    private async Task CreateAndSendOtpAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var existingOtps = await _dbContext.EmailOtps
            .Where(item => item.UserId == user.Id && !item.IsUsed)
            .ToListAsync(cancellationToken);

        if (existingOtps.Count > 0)
        {
            _dbContext.EmailOtps.RemoveRange(existingOtps);
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString(CultureInfo.InvariantCulture);
        _dbContext.EmailOtps.Add(new EmailOtp
        {
            UserId = user.Id,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var content = $"""
                       <div style="font-family:Segoe UI,Arial,sans-serif;line-height:1.6;color:#3d2c25">
                           <h2 style="margin-bottom:8px;">Mã OTP xác nhận tài khoản Share Food</h2>
                           <p>Xin chào <strong>{user.FullName}</strong>,</p>
                           <p>Mã OTP để kích hoạt tài khoản của bạn là:</p>
                           <div style="font-size:32px;font-weight:700;letter-spacing:6px;background:#fff6ec;border:1px solid #f2d4bd;border-radius:16px;padding:18px 22px;display:inline-block;">
                               {code}
                           </div>
                           <p style="margin-top:16px;">Mã có hiệu lực trong vòng <strong>10 phút</strong>. Nếu bạn không thực hiện thao tác này, hãy bỏ qua email.</p>
                       </div>
                       """;

        await _emailService.SendEmailAsync(user.Email!, user.FullName, "Mã OTP xác nhận tài khoản Share Food", content, cancellationToken);
    }

    private async Task SendWelcomeEmailAsync(ApplicationUser user)
    {
        var loginUrl = Url.Action(nameof(Login), "Account", new { area = "Client" }, Request.Scheme);
        var content = $"""
                       <div style="font-family:Segoe UI,Arial,sans-serif;line-height:1.7;color:#2f2520">
                           <h2 style="margin-bottom:10px;">Chào mừng bạn đến với Share Food</h2>
                           <p>Xin chào <strong>{user.FullName}</strong>,</p>
                           <p>Tài khoản của bạn đã được kích hoạt thành công. Từ bây giờ bạn có thể đăng công thức, bình luận và lưu lại những món ăn yêu thích.</p>
                           <p>
                               <a href="{loginUrl}" style="background:#c96b3b;color:#fff;padding:12px 18px;text-decoration:none;border-radius:10px;display:inline-block;">
                                   Đăng nhập ngay
                               </a>
                           </p>
                           <p>Chúc bạn có thật nhiều cảm hứng trong gian bếp cùng cộng đồng Share Food.</p>
                       </div>
                       """;

        await _emailService.SendEmailAsync(user.Email!, user.FullName, "Chào mừng bạn đến với Share Food", content);
    }

    private async Task SendGettingStartedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var latestRecipes = await _dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.IsVisible)
            .OrderByDescending(item => item.CreatedAt)
            .Take(3)
            .Select(item => new
            {
                item.Title,
                item.Id,
                item.Slug
            })
            .ToListAsync(cancellationToken);

        if (latestRecipes.Count == 0)
        {
            return;
        }

        var recipeLinks = latestRecipes
            .Select(item =>
            {
                var url = Url.Action("Details", "Recipes", new { area = "Client", id = item.Id, slug = item.Slug }, Request.Scheme);
                return $"""<li style="margin-bottom:8px;"><a href="{url}" style="color:#8f4327;text-decoration:none;">{item.Title}</a></li>""";
            });

        var content = $"""
                       <div style="font-family:Segoe UI,Arial,sans-serif;line-height:1.7;color:#2f2520">
                           <h2 style="margin-bottom:10px;">Gợi ý món mới dành cho bạn</h2>
                           <p>Đây là những công thức đang nổi bật trên Share Food để bạn bắt đầu khám phá:</p>
                           <ul style="padding-left:18px;">
                               {string.Join("", recipeLinks)}
                           </ul>
                           <p>Hãy lưu lại món bạn thích hoặc chia sẻ ngay công thức đầu tay của bạn với cộng đồng.</p>
                       </div>
                       """;

        await _emailService.SendEmailAsync(user.Email!, user.FullName, "Khám phá công thức mới trên Share Food", content, cancellationToken);
    }
}
