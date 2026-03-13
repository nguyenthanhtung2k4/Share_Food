using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Helpers;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.ViewModels.Admin;

namespace ShareFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("quan-tri/nguoi-dung")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? keyword, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(item => item.FullName.Contains(keyword) || item.Email!.Contains(keyword));
        }

        var items = await query
            .OrderByDescending(item => item.JoinedAt)
            .Select(item => new UserAdminListItemViewModel
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email ?? string.Empty,
                EmailConfirmed = item.EmailConfirmed,
                IsLocked = item.LockoutEnd.HasValue && item.LockoutEnd > DateTimeOffset.UtcNow,
                RecipeCount = item.Recipes.Count,
                JoinedAt = item.JoinedAt
            })
            .ToListAsync(cancellationToken);

        ViewData["Keyword"] = keyword;
        return View(items);
    }

    [HttpPost("khoa-mo/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, id, StringComparison.Ordinal))
        {
            TempData.SetError("Không thể thực hiện", "Bạn không thể tự khóa tài khoản của mình.");
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData.SetError("Không tìm thấy", "Người dùng cần cập nhật không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            TempData.SetError("Không thể thực hiện", "Không thể khóa tài khoản quản trị viên.");
            return RedirectToAction(nameof(Index));
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            ? DateTimeOffset.UtcNow
            : DateTimeOffset.UtcNow.AddYears(10);

        await _userManager.UpdateAsync(user);
        TempData.SetSuccess(
            "Cập nhật thành công",
            user.LockoutEnd > DateTimeOffset.UtcNow ? "Tài khoản đã bị khóa." : "Tài khoản đã được mở khóa.");

        return RedirectToAction(nameof(Index));
    }
}
