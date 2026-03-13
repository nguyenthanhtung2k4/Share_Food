using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ShareFood.Web.ViewModels.Common;

namespace ShareFood.Web.Helpers;

public static class TempDataExtensions
{
    private const string TitleKey = "Toast.Title";
    private const string MessageKey = "Toast.Message";
    private const string CssClassKey = "Toast.CssClass";

    public static void SetSuccess(this ITempDataDictionary tempData, string title, string message)
    {
        SetToast(tempData, title, message, "bg-success-subtle text-success-emphasis");
    }

    public static void SetError(this ITempDataDictionary tempData, string title, string message)
    {
        SetToast(tempData, title, message, "bg-danger-subtle text-danger-emphasis");
    }

    public static void SetInfo(this ITempDataDictionary tempData, string title, string message)
    {
        SetToast(tempData, title, message, "bg-warning-subtle text-warning-emphasis");
    }

    public static ToastMessage? GetToast(this ITempDataDictionary tempData)
    {
        if (!tempData.TryGetValue(TitleKey, out var title) || !tempData.TryGetValue(MessageKey, out var message))
        {
            return null;
        }

        return new ToastMessage
        {
            Title = title?.ToString() ?? string.Empty,
            Message = message?.ToString() ?? string.Empty,
            CssClass = tempData[CssClassKey]?.ToString() ?? "bg-success-subtle text-success-emphasis"
        };
    }

    private static void SetToast(ITempDataDictionary tempData, string title, string message, string cssClass)
    {
        tempData[TitleKey] = title;
        tempData[MessageKey] = message;
        tempData[CssClassKey] = cssClass;
    }
}
