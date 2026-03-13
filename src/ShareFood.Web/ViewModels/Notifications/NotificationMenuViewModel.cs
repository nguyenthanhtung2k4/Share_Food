using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Notifications;

public class NotificationMenuViewModel
{
    public int UnreadCount { get; init; }

    public IReadOnlyList<NotificationItemViewModel> Items { get; init; } = Array.Empty<NotificationItemViewModel>();
}
