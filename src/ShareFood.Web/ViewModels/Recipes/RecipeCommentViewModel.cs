namespace ShareFood.Web.ViewModels.Recipes;

public class RecipeCommentViewModel
{
    public string UserName { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
