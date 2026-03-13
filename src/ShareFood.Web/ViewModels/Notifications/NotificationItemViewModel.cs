namespace ShareFood.Web.ViewModels.Notifications;

public class NotificationItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public bool IsRead { get; init; }

    public int RecipeId { get; init; }

    public string RecipeSlug { get; init; } = string.Empty;
}
