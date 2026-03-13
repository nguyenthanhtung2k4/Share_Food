using ShareFood.Web.ViewModels.Recipes;
using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Profile;

public class ProfileOverviewViewModel
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime JoinedAt { get; init; }

    public int RecipeCount { get; init; }

    public int FavoriteCount { get; init; }

    public IReadOnlyList<RecipeCardViewModel> MyRecipes { get; init; } = Array.Empty<RecipeCardViewModel>();
}
