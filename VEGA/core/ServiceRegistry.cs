namespace Core
{
    /// <summary>
    /// Simple static registry to expose runtime singletons to parts of the app that are not created via DI
    /// </summary>
    public static class GlobalRegistry
    {
        public static DateTime StartTime { get; } = DateTime.UtcNow;

        
        private static IServiceProvider? _mainServiceProvider;
        public static IServiceProvider MainServiceProvider =>
            _mainServiceProvider ?? throw new InvalidOperationException("MainServiceProvider not set. Set it in Program.cs before using.");

        public static void SetMainServiceProvider(IServiceProvider provider) => _mainServiceProvider = provider;
    }
}
