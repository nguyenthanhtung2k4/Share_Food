namespace ShareFood.Web.ViewModels.Admin;

public class UserAdminListItemViewModel
{
    public string Id { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool EmailConfirmed { get; init; }

    public bool IsLocked { get; init; }

    public int RecipeCount { get; init; }

    public DateTime JoinedAt { get; init; }
}
