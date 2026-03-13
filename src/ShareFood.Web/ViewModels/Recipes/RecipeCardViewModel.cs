namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeCardViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string? ThumbnailPath { get; init; }

    public int CookTimeMinutes { get; init; }

    public int Servings { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string AuthorName { get; init; } = string.Empty;

    public int FavoriteCount { get; init; }

    public int CommentCount { get; init; }
}
