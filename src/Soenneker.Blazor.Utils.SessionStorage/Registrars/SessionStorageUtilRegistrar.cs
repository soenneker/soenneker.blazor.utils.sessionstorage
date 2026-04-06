using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;

namespace Soenneker.Blazor.Utils.SessionStorage.Registrars;

/// <summary>
/// Registration for the interop and utility services.
/// </summary>
public static class SessionStorageUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ISessionStorageInterop"/> and <see cref="ISessionStorageUtil"/> as scoped services.
    /// </summary>
    public static IServiceCollection AddSessionStorageUtilAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped()
                .TryAddScoped<ISessionStorageInterop, SessionStorageInterop>();

        services.TryAddScoped<ISessionStorageUtil, SessionStorageUtil>();

        return services;
    }
}
