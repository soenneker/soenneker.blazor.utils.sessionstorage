using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.Utils.SessionStorage.Abstract;

/// <summary>
/// A Blazor utility for browser <c>sessionStorage</c> backed by Librarian.
/// </summary>
/// <remarks>Uses a dedicated Librarian snapshot. Existing raw browser entries are not migrated.
/// Mutations persist before returning; writes require Web Locks in a secure context.
/// Overlapping or external writes can raise LibrarianConcurrencyException; failed operations are not automatically retried.
/// Clear, GetKeys, and GetLength apply only to documents belonging to this utility.</remarks>
public interface ISessionStorageUtil : IAsyncDisposable
{
    /// <summary>Prevents new operations. Operations already started retain their own database and finish independently.</summary>
    new ValueTask DisposeAsync();

    /// <summary>
    /// Opens the Librarian document container after interactive rendering.
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
    [RequiresUnreferencedCode("JSON deserialization uses reflection. Use the overload accepting JsonTypeInfo<T> for trimming.")]
    [RequiresDynamicCode("JSON deserialization may require runtime code generation. Use the overload accepting JsonTypeInfo<T> for AOT.")]
    ValueTask<T?> Get<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a JSON-serialized value by key, or default if the key does not exist.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the session storage.</typeparam>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="typeInfo">Generated JSON metadata for the value type.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the value returned by get.</returns>
    ValueTask<T?> Get<T>(string key, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value for the specified key.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">String value to store under the specified key.</param>
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
    [RequiresUnreferencedCode("JSON serialization uses reflection. Use the overload accepting JsonTypeInfo<T> for trimming.")]
    [RequiresDynamicCode("JSON serialization may require runtime code generation. Use the overload accepting JsonTypeInfo<T> for AOT.")]
    ValueTask Set<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a JSON-serialized value for the specified key.
    /// </summary>
    /// <typeparam name="T">Type of value handled by the session storage.</typeparam>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="value">Value to serialize and store under the specified key.</param>
    /// <param name="typeInfo">Generated JSON metadata for the value type.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the set operation is complete.</returns>
    ValueTask Set<T>(string key, T value, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stored value by key.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the remove operation is complete.</returns>
    ValueTask Remove(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all documents belonging to this utility and persists the change.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the Session Storage has been cleared.</returns>
    ValueTask Clear(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the specified key exists in the utility document container.
    /// </summary>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>true if the specified key exists in the target store; otherwise, false.</returns>
    ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all document keys belonging to this utility in unspecified order.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The storage keys in unspecified order.</returns>
    ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of documents belonging to this utility.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested value.</returns>
    ValueTask<int> GetLength(CancellationToken cancellationToken = default);
}
