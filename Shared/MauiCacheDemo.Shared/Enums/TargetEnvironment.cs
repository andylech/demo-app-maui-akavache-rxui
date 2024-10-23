using System.Diagnostics.CodeAnalysis;

namespace MauiCacheDemo.Shared.Enums
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TargetEnvironment
    {
        // Placeholder/Fake
        Unknown,

        // Remote
        Dev,
        QA,
        Prod,

        // Local
        AndroidLoopbackIis,
        LocalhostIis,
        LocalhostNode,
        LocalhostRest,
        MachineIpIis
    }
}
