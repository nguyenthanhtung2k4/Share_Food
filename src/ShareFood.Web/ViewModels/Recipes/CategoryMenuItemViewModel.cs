namespace ShareFood.Web.ViewModels.Recipes;

public class CategoryMenuItemViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public int RecipeCount { get; init; }
}
