using MauiCacheDemo.Shared.Enums;

namespace MauiCacheDemo.Shared.Helpers;

public static class TargetEnvironmentExtensions
{
    private const string AndroidLoopbackIisStr = "Android Loopback";

    private const string LocalhostIisStr = "Localhost IIS";

    private const string LocalhostNodeStr = "Localhost Node";

    private const string LocalhostRestStr = "Localhost REST";

    private const string MachineIpIisStr = "Machine IP";

    public static string ToName(this TargetEnvironment targetEnvironment)
    {
        return targetEnvironment switch
        {
            TargetEnvironment.AndroidLoopbackIis => AndroidLoopbackIisStr,
            TargetEnvironment.LocalhostIis => LocalhostIisStr,
            TargetEnvironment.LocalhostNode => LocalhostNodeStr,
            TargetEnvironment.LocalhostRest => LocalhostRestStr,
            TargetEnvironment.MachineIpIis => MachineIpIisStr,
            _ => targetEnvironment.ToString()
        };
    }
}
