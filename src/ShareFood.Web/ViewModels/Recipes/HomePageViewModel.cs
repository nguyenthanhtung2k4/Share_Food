using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Recipes;

public class HomePageViewModel
{
    public IReadOnlyList<RecipeCardViewModel> FeaturedRecipes { get; init; } = Array.Empty<RecipeCardViewModel>();

    public IReadOnlyList<RecipeCardViewModel> LatestRecipes { get; init; } = Array.Empty<RecipeCardViewModel>();
}
