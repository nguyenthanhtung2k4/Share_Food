using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.ViewModels.Profile;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize]
[Route("ho-so")]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ProfileController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
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

        return View(new ProfileOverviewViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            JoinedAt = user.JoinedAt,
            RecipeCount = myRecipes.Count,
            FavoriteCount = await _dbContext.Favorites.CountAsync(item => item.UserId == userId, cancellationToken),
            MyRecipes = myRecipes
        });
    }
}
