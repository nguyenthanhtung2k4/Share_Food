using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.ViewModels.Notifications;

namespace ShareFood.Web.ViewComponents;

public class NotificationMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationMenuViewComponent(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 5)
    {
        var principal = HttpContext?.User;
        if (!(principal?.Identity?.IsAuthenticated ?? false))
        {
            return Content(string.Empty);
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Content(string.Empty);
        }

        var unreadCount = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(item => item.RecipientUserId == userId && !item.IsRead);

        var items = await _dbContext.Notifications
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
            .Take(count)
            .ToListAsync();

        return View(new NotificationMenuViewModel
        {
            UnreadCount = unreadCount,
            Items = items
        });
    }
}
