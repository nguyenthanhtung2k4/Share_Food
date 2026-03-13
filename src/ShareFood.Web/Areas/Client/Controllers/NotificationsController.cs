using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.ViewModels.Notifications;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
[Route("thong-bao")]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.RecipientUserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new NotificationItemViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Message = item.Message,
                CreatedAt = item.CreatedAt,
                IsRead = item.IsRead,
                RecipeId = item.RecipeId,
                RecipeSlug = item.Recipe!.Slug
            })
            .ToListAsync(cancellationToken);

        return View(new NotificationPageViewModel
        {
            UnreadCount = notifications.Count(item => !item.IsRead),
            Notifications = notifications
        });
    }

    [HttpGet("mo/{id:int}")]
    public async Task<IActionResult> Open(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var notification = await _dbContext.Notifications
            .Include(item => item.Recipe)
            .FirstOrDefaultAsync(item => item.Id == id && item.RecipientUserId == userId, cancellationToken);

        if (notification is null)
        {
            TempData.SetError("Không tìm thấy", "Thông báo bạn cần xem không tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (notification.Recipe is null)
        {
            TempData.SetInfo("Thông báo đã đọc", "Công thức liên quan không còn tồn tại.");
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction("Details", "Recipes", new
        {
            area = "Client",
            id = notification.RecipeId,
            slug = notification.Recipe.Slug
        });
    }

    [HttpPost("danh-dau-da-doc")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var items = await _dbContext.Notifications
            .Where(item => item.RecipientUserId == userId && !item.IsRead)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            TempData.SetInfo("Không có thay đổi", "Bạn hiện không còn thông báo chưa đọc.");
            return RedirectToAction(nameof(Index));
        }

        foreach (var item in items)
        {
            item.IsRead = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData.SetSuccess("Cập nhật thành công", "Tất cả thông báo đã được đánh dấu là đã đọc.");
        return RedirectToAction(nameof(Index));
    }
}
