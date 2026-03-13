namespace ShareFood.Web.ViewModels.Common;

public class ToastMessage
{
    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string CssClass { get; init; } = "bg-success-subtle text-success-emphasis";
}
