namespace MauiCacheDemo.Core.Enums;

public enum PageType
{
    // Numbering schema:
    // Loading pages (splash, update, tutorial, etc.) < 100
    // Primary pages (top-level navigation targets or standalone) / 100 = 0
    // Secondary pages (follows selection on primary pages) % 100 = 1 to 99

    Splash = 0,

    Login = 100,

    // ...
}
