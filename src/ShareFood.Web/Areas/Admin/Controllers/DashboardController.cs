using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.ViewModels.Admin;

namespace ShareFood.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("quan-tri")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = new DashboardViewModel
        {
            TotalUsers = await _dbContext.Users.CountAsync(cancellationToken),
            TotalRecipes = await _dbContext.Recipes.CountAsync(cancellationToken),
            VisibleRecipes = await _dbContext.Recipes.CountAsync(item => item.IsVisible, cancellationToken),
            TotalComments = await _dbContext.Comments.CountAsync(cancellationToken),
            LatestRecipes = await _dbContext.Recipes
                .AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Take(6)
                .Select(item => new DashboardRecipeViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    AuthorName = item.Author!.FullName,
                    IsVisible = item.IsVisible,
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync(cancellationToken)
        };

        return View(viewModel);
    }
}
