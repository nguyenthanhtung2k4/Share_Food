using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Helpers;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
[Route("yeu-thich")]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public FavoritesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var items = await _dbContext.Favorites
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.Recipe!.IsVisible)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new RecipeCardViewModel
            {
                Id = item.Recipe!.Id,
                Title = item.Recipe.Title,
                Slug = item.Recipe.Slug,
                Summary = item.Recipe.Summary,
                ThumbnailPath = item.Recipe.ThumbnailPath,
                CookTimeMinutes = item.Recipe.CookTimeMinutes,
                Servings = item.Recipe.Servings,
                CategoryName = item.Recipe.Category!.Name,
                AuthorName = item.Recipe.Author!.FullName,
                FavoriteCount = item.Recipe.Favorites.Count,
                CommentCount = item.Recipe.Comments.Count
            })
            .ToListAsync(cancellationToken);

        return View(items);
    }

    [HttpPost("chuyen-doi/{recipeId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int recipeId, string? returnUrl, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var actorName = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => item.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == recipeId && item.IsVisible, cancellationToken);

        if (recipe is null)
        {
            TempData.SetError("Không tìm thấy", "Công thức bạn muốn lưu không tồn tại.");
            return RedirectToAction("Index", "Recipes");
        }

        var favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(item => item.RecipeId == recipeId && item.UserId == userId, cancellationToken);

        if (favorite is null)
        {
            _dbContext.Favorites.Add(new Models.Entities.Favorite
            {
                RecipeId = recipeId,
                UserId = userId
            });

            if (!string.Equals(recipe.AuthorId, userId, StringComparison.Ordinal))
            {
                _dbContext.Notifications.Add(new Models.Entities.Notification
                {
                    RecipientUserId = recipe.AuthorId,
                    RecipeId = recipeId,
                    Title = "Có người thích công thức của bạn",
                    Message = $"{actorName ?? "Một thành viên"} vừa lưu công thức \"{recipe.Title}\" của bạn vào danh sách yêu thích."
                });
            }

            TempData.SetSuccess("Đã lưu công thức", "Công thức đã được thêm vào danh sách yêu thích.");
        }
        else
        {
            _dbContext.Favorites.Remove(favorite);
            TempData.SetInfo("Đã bỏ yêu thích", "Công thức đã được gỡ khỏi danh sách yêu thích.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Details", "Recipes", new { id = recipe.Id, slug = recipe.Slug });
    }
}
