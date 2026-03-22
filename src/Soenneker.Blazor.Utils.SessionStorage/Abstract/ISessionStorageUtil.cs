using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.SessionStorage.Abstract;

/// <summary>
/// A higher-level Blazor utility for browser <c>sessionStorage</c> built on top of <see cref="ISessionStorageInterop"/>.
/// </summary>
public interface ISessionStorageUtil
{
    /// <summary>
    /// Ensures the underlying JavaScript module has been loaded and is ready for use.
    /// </summary>
    ValueTask Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a stored string value by key, or null if the key does not exist.
    /// </summary>
    ValueTask<string?> Get(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a JSON-serialized value by key, or default if the key does not exist.
    /// </summary>
    ValueTask<T?> Get<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value for the specified key.
    /// </summary>
    ValueTask Set(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a JSON-serialized value for the specified key.
    /// </summary>
    ValueTask Set<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored value by key.
    /// </summary>
    ValueTask Remove(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all browser session storage entries.
    /// </summary>
    ValueTask Clear(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the specified key exists in browser session storage.
    /// </summary>
    ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all session storage keys in index order.
    /// </summary>
    ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of session storage entries.
    /// </summary>
    ValueTask<int> GetLength(CancellationToken cancellationToken = default);
}
