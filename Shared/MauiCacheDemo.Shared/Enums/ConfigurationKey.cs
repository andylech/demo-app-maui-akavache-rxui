namespace MauiCacheDemo.Shared.Enums
{
    // TODO Replace with strings to allow for hierarchy of config values
    public enum ConfigurationKey
    {
        //
        // These config values determine which API URL value below is used
        //

        LiveApiEnvironment,
        TestApiEnvironment,

        //
        // API source keys depending on given Environment and Platform values
        //

        // Remote

        DebugApiUrl,
        QaApiUrl,
        ProdApiUrl,

        // Local

        // Special Android Loopback URL to Localhost with same Port as IIS
        AndroidLoopbackIisUrl,
        // AndroidLoopbackIisUrl will be used when Android and emulator
        // MachineIpIisUrl will be used when iOS or Android and physical
        LocalhostIisUrl,
        // API running locally (only works for Postman and API test client)
        LocalhostNodeUrl,
        LocalhostRestUrl,
        // Ip of local machine running API for iOS or Android physical devices
        MachineIpIisUrl,

        //
        // Test values (API Smoke Tester)
        //

        TestApiUserId,

        //
        // Third-party service keys
        //

        AppCenterAndroidDebugQa,
        AppCenterAndroidReleaseProd,
        AppCenterAndroidAdHocProd,
        AppCenterAndroidAppStoreProd,

        AppCenterIosDebugQa,
        AppCenterIosReleaseProd,
        AppCenterIosAdHocProd,
        AppCenterIosAppStoreProd
    }
}
