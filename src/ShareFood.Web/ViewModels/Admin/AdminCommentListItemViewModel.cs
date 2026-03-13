namespace ShareFood.Web.ViewModels.Admin;

public class AdminCommentListItemViewModel
{
    public int Id { get; init; }

    public int RecipeId { get; init; }

    public string RecipeTitle { get; init; } = string.Empty;

    public string RecipeSlug { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
