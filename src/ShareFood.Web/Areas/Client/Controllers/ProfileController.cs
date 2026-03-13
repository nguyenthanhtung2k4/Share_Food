using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.ViewModels.Profile;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
[Route("ho-so")]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var viewModel = await BuildProfileViewModelAsync(userId, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpPost("cap-nhat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([Bind(Prefix = "EditProfile")] ProfileEditViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var invalidViewModel = await BuildProfileViewModelAsync(userId, cancellationToken, editProfile: model);
            return View("Index", invalidViewModel);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.ReceiveNewsEmails = model.ReceiveNewsEmails;

        await _userManager.UpdateAsync(user);
        TempData.SetSuccess("Cập nhật thành công", "Thông tin cá nhân đã được lưu.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("doi-mat-khau")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            var invalidViewModel = await BuildProfileViewModelAsync(userId, cancellationToken, changePassword: model);
            return View("Index", invalidViewModel);
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var invalidViewModel = await BuildProfileViewModelAsync(userId, cancellationToken, changePassword: model);
            return View("Index", invalidViewModel);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData.SetSuccess("Đổi mật khẩu thành công", "Mật khẩu của bạn đã được cập nhật.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProfileOverviewViewModel?> BuildProfileViewModelAsync(
        string userId,
        CancellationToken cancellationToken,
        ProfileEditViewModel? editProfile = null,
        ChangePasswordViewModel? changePassword = null)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var myRecipes = await _dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.AuthorId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new RecipeCardViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Slug = item.Slug,
                Summary = item.Summary,
                ThumbnailPath = item.ThumbnailPath,
                CookTimeMinutes = item.CookTimeMinutes,
                Servings = item.Servings,
                CategoryName = item.Category!.Name,
                AuthorName = user.FullName,
                FavoriteCount = item.Favorites.Count,
                CommentCount = item.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return new ProfileOverviewViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            ReceiveNewsEmails = user.ReceiveNewsEmails,
            JoinedAt = user.JoinedAt,
            RecipeCount = myRecipes.Count,
            FavoriteCount = await _dbContext.Favorites.CountAsync(item => item.UserId == userId, cancellationToken),
            EditProfile = editProfile ?? new ProfileEditViewModel
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ReceiveNewsEmails = user.ReceiveNewsEmails
            },
            ChangePassword = changePassword ?? new ChangePasswordViewModel(),
            MyRecipes = myRecipes
        };
    }
}
