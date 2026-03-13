namespace ShareFood.Web.ViewModels.Admin;

public class AdminRecipeListItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string AuthorName { get; init; } = string.Empty;

    public bool IsVisible { get; init; }

    public DateTime CreatedAt { get; init; }
}
