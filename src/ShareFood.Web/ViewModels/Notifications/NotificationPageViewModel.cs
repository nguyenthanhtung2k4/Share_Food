using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Notifications;

public class NotificationPageViewModel
{
    public int UnreadCount { get; init; }

    public IReadOnlyList<NotificationItemViewModel> Notifications { get; init; } = Array.Empty<NotificationItemViewModel>();
}
