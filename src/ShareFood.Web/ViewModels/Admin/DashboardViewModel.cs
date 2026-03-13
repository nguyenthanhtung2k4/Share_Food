using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Admin;

public class DashboardViewModel
{
    public int TotalUsers { get; init; }

    public int TotalRecipes { get; init; }

    public int VisibleRecipes { get; init; }

    public int TotalComments { get; init; }

    public IReadOnlyList<DashboardRecipeViewModel> LatestRecipes { get; init; } = Array.Empty<DashboardRecipeViewModel>();
}
