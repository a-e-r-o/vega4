using System;

namespace Core
{
    /// <summary>
    /// Simple static registry to expose runtime singletons to parts of the app
    /// that are not created via DI
    /// </summary>
    public static class ServiceRegistry
    {
        public static IServiceProvider? ServiceProvider { get; set; }
    }
}
