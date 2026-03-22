using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;

namespace Soenneker.Blazor.Utils.SessionStorage;

/// <inheritdoc cref="ISessionStorageUtil"/>
public sealed class SessionStorageUtil : ISessionStorageUtil
{
    private readonly ISessionStorageInterop _interop;
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public SessionStorageUtil(ISessionStorageInterop interop)
    {
        _interop = interop ?? throw new ArgumentNullException(nameof(interop));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        return _interop.Initialize(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<string?> Get(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return _interop.Get(key, cancellationToken);
    }

    public async ValueTask<T?> Get<T>(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        string? value = await _interop.Get(key, cancellationToken)
                                      .ConfigureAwait(false);

        if (value is null)
            return default;

        if (typeof(T) == typeof(string))
            return (T?) (object) value;

        if (string.IsNullOrWhiteSpace(value))
            return default;

        return JsonSerializer.Deserialize<T>(value, _serializerOptions);
    }

    public ValueTask Set(string key, string value, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(value);

        return _interop.Set(key, value, cancellationToken);
    }

    public ValueTask Set<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        if (value is string stringValue)
            return _interop.Set(key, stringValue, cancellationToken);

        string json = JsonSerializer.Serialize(value, _serializerOptions);
        return _interop.Set(key, json, cancellationToken);
    }

    public ValueTask Remove(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return _interop.Remove(key, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Clear(CancellationToken cancellationToken = default)
    {
        return _interop.Clear(cancellationToken);
    }

    public ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return _interop.ContainsKey(key, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default)
    {
        return _interop.GetKeys(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<int> GetLength(CancellationToken cancellationToken = default)
    {
        return _interop.GetLength(cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Session storage key cannot be null or whitespace.", nameof(key));
    }
}
