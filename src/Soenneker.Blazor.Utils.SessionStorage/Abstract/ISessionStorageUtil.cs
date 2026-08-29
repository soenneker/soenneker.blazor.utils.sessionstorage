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
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the Session Storage is ready for use.</returns>
    ValueTask Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a stored string value by key, or null if the key does not exist.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the text returned by get.</returns>
    ValueTask<string?> Get(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a JSON-serialized value by key, or default if the key does not exist.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the session storage.</typeparam>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the value returned by get.</returns>
    ValueTask<T?> Get<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value for the specified key.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">Value to serialize and store under the specified key.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the set operation is complete.</returns>
    ValueTask Set(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a JSON-serialized value for the specified key.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the session storage.</typeparam>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">Value to serialize and store under the specified key.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the set operation is complete.</returns>
    ValueTask Set<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored value by key.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the remove operation is complete.</returns>
    ValueTask Remove(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all browser session storage entries.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the Session Storage has been cleared.</returns>
    ValueTask Clear(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the specified key exists in browser session storage.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>true if the specified key exists in the target store; otherwise, false.</returns>
    ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all session storage keys in index order.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the collection returned by get Keys.</returns>
    ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of session storage entries.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested value.</returns>
    ValueTask<int> GetLength(CancellationToken cancellationToken = default);
}
