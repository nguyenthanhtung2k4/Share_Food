using System.Collections.Generic;

namespace ShareFood.Web.ViewModels.Common;

public class PaginationViewModel
{
    public int PageNumber { get; init; }

    public int TotalPages { get; init; }

    public string Action { get; init; } = string.Empty;

    public string Controller { get; init; } = string.Empty;

    public string? Area { get; init; }

    public Dictionary<string, string?> RouteValues { get; init; } = new();
}
