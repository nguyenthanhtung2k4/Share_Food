using ShareFood.Web.ViewModels.Common;
using ShareFood.Web.ViewModels.Recipes;

namespace ShareFood.Web.Services;

public interface IRecipeQueryService
{
    Task<PagedResult<RecipeCardViewModel>> SearchVisibleRecipesAsync(string? keyword, int? categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecipeCardViewModel>> GetLatestRecipesAsync(int count, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecipeCardViewModel>> GetFeaturedRecipesAsync(int count, CancellationToken cancellationToken = default);
}
