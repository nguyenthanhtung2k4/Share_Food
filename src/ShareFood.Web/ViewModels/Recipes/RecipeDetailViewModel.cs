using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeDetailViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string? ThumbnailPath { get; init; }

    public int Servings { get; init; }

    public int CookTimeMinutes { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string AuthorId { get; init; } = string.Empty;

    public string AuthorName { get; init; } = string.Empty;

    public bool IsVisible { get; init; }

    public bool IsOwner { get; init; }

    public bool IsFavorited { get; init; }

    public int FavoriteCount { get; init; }

    public IReadOnlyList<RecipeIngredientDetailViewModel> Ingredients { get; init; } = Array.Empty<RecipeIngredientDetailViewModel>();

    public IReadOnlyList<RecipeStepDetailViewModel> Steps { get; init; } = Array.Empty<RecipeStepDetailViewModel>();

    public IReadOnlyList<RecipeCommentViewModel> Comments { get; init; } = Array.Empty<RecipeCommentViewModel>();

    public CommentInputViewModel NewComment { get; init; } = new();
}
