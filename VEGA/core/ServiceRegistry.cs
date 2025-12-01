namespace Core
{
    /// <summary>
    /// Simple static registry to expose runtime singletons to parts of the app that are not created via DI
    /// </summary>
    public static class GlobalRegistry
    {
        public static DateTime StartTime { get; } = DateTime.UtcNow;
        public static IServiceProvider? MainServiceProvider { get; set; }
    }
}
