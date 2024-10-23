using Splat;

namespace MauiCacheDemo.Core.Enums;

public static class PageTypeHelper
{
    public static string GetTitle(this PageType pageType)
    {
        string pageTitle;

        try
        {
            pageTitle = pageType switch
            {
                PageType.Splash => "Splash",
                PageType.Login => "Login",
                _ => pageType.ToString()
            };
        }
        catch (Exception exception)
        {
            LogHost.Default.Error(exception,
                $"EXCEPTION:  {exception.Message}");

            pageTitle = "Exception";
        }

        return pageTitle;
    }
}
