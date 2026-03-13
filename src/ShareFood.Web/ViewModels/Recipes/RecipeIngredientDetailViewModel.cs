namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeIngredientDetailViewModel
{
    public string IngredientName { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public string UnitName { get; init; } = string.Empty;

    public string UnitSymbol { get; init; } = string.Empty;

    public string? Note { get; init; }
}
