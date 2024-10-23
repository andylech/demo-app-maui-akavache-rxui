using System.Diagnostics.CodeAnalysis;

namespace MauiCacheDemo.Shared.Enums;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum TargetPlatform
{
    Unknown,
    Android,
    iOS,
    Windows,
    Console
}
