using Microsoft.EntityFrameworkCore;
using ShareFood.Web.Data;
using ShareFood.Web.Models.Entities;
using ShareFood.Web.ViewModels.Common;
using ShareFood.Web.ViewModels.Recipes;
using System.Linq.Expressions;

namespace ShareFood.Web.Services;

public class RecipeQueryService : IRecipeQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public RecipeQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecipeCardViewModel>> GetFeaturedRecipesAsync(int count, CancellationToken cancellationToken = default)
    {
        var items = await QueryVisibleRecipes()
            .OrderByDescending(recipe => recipe.Favorites.Count)
            .ThenByDescending(recipe => recipe.CreatedAt)
            .Take(count)
            .Select(MapRecipeCard())
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<IReadOnlyList<RecipeCardViewModel>> GetLatestRecipesAsync(int count, CancellationToken cancellationToken = default)
    {
        var items = await QueryVisibleRecipes()
            .OrderByDescending(recipe => recipe.CreatedAt)
            .Take(count)
            .Select(MapRecipeCard())
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<PagedResult<RecipeCardViewModel>> SearchVisibleRecipesAsync(string? keyword, int? categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedKeyword = keyword?.Trim().ToLower();
        var query = QueryVisibleRecipes();

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            query = query.Where(recipe =>
                recipe.Title.ToLower().Contains(normalizedKeyword) ||
                recipe.Summary.ToLower().Contains(normalizedKeyword) ||
                recipe.Category!.Name.ToLower().Contains(normalizedKeyword));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(recipe => recipe.CategoryId == categoryId.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(recipe => recipe.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapRecipeCard())
            .ToListAsync(cancellationToken);

        return new PagedResult<RecipeCardViewModel>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<Recipe> QueryVisibleRecipes()
    {
        return _dbContext.Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Author)
            .Include(recipe => recipe.Comments)
            .Include(recipe => recipe.Favorites)
            .Where(recipe => recipe.IsVisible);
    }

    private static Expression<Func<Recipe, RecipeCardViewModel>> MapRecipeCard()
    {
        return recipe => new RecipeCardViewModel
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Slug = recipe.Slug,
            Summary = recipe.Summary,
            ThumbnailPath = recipe.ThumbnailPath,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            CategoryName = recipe.Category!.Name,
            AuthorName = recipe.Author!.FullName,
            FavoriteCount = recipe.Favorites.Count,
            CommentCount = recipe.Comments.Count
        };
    }
}
