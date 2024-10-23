namespace MauiCacheDemo.Shared.Helpers
{
    public static class EnumExtensions
    {
        // From https://makolyte.com/csharp-enum-generic-type-constraint/
        public static T Parse<T>(this string enumStr) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), enumStr);
        }
    }
}
