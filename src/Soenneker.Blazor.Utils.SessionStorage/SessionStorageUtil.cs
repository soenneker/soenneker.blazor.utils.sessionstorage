using Soenneker.Extensions.ValueTask;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Librarian.Abstractions;
using Soenneker.Librarian.SessionStorage;

namespace Soenneker.Blazor.Utils.SessionStorage;

public sealed class SessionStorageUtil : ISessionStorageUtil
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private const string _containerName = "entries";
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly ILogger<SessionStorageLibrarianDatabase> _logger;
    private volatile bool _disposed;

    public SessionStorageUtil(IModuleImportUtil moduleImportUtil, ILogger<SessionStorageLibrarianDatabase> logger)
    {
        _moduleImportUtil = moduleImportUtil ?? throw new ArgumentNullException(nameof(moduleImportUtil));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask Initialize(CancellationToken cancellationToken = default) =>
        _ = await Run(container => ValueTask.FromResult(container), cancellationToken).NoSync();

    public ValueTask<string?> Get(string key, CancellationToken cancellationToken = default)
    {
        string id = EncodeKey(key);
        return Run(async container =>
        {
            string? json = await container.GetItem(id, cancellationToken).NoSync();
            return json is null ? null : JsonSerializer.Deserialize(json, StorageJsonContext.Default.StorageDocument)!.Value;
        }, cancellationToken);
    }

    public async ValueTask Set(string key, string value, CancellationToken cancellationToken = default)
    {
        string id = EncodeKey(key);
        ArgumentNullException.ThrowIfNull(value);
        string json = JsonSerializer.Serialize(new StorageDocument(value), StorageJsonContext.Default.StorageDocument);
        await Run(async container =>
        {
            if (await container.UpdateItem(id, json, cancellationToken).NoSync() is null)
                await container.AddItem(id, json, cancellationToken).NoSync();
            return true;
        }, cancellationToken, save: true).NoSync();
    }

    public async ValueTask Remove(string key, CancellationToken cancellationToken = default)
    {
        string id = EncodeKey(key);
        await Run(async container =>
        {
            await container.DeleteItem(id, cancellationToken).NoSync();
            return true;
        }, cancellationToken, save: true).NoSync();
    }

    public async ValueTask Clear(CancellationToken cancellationToken = default)
    {
        await Run(async container =>
        {
            await container.DeleteAllItems(cancellationToken).NoSync();
            return true;
        }, cancellationToken, save: true).NoSync();
    }

    public async ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default) =>
        await Get(key, cancellationToken).NoSync() is not null;

    public async ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default)
    {
        return await Run(async container =>
        {
            List<string> ids = await container.GetAllIds(cancellationToken).NoSync();
            for (var i = 0; i < ids.Count; i++)
                ids[i] = DecodeKey(ids[i]);
            return ids;
        }, cancellationToken).NoSync();
    }

    public ValueTask<int> GetLength(CancellationToken cancellationToken = default) =>
        Run(container => container.CountItems(cancellationToken), cancellationToken);

    [RequiresUnreferencedCode("JSON deserialization uses reflection. Use the overload accepting JsonTypeInfo<T> for trimming.")]
    [RequiresDynamicCode("JSON deserialization may require runtime code generation. Use the overload accepting JsonTypeInfo<T> for AOT.")]
    public async ValueTask<T?> Get<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string? value = await Get(key, cancellationToken)
                                      .NoSync();

        if (value is null)
            return default;

        if (typeof(T) == typeof(string))
            return (T?) (object) value;

        if (string.IsNullOrWhiteSpace(value))
            return default;

        return JsonSerializer.Deserialize<T>(value, _serializerOptions);
    }

    public async ValueTask<T?> Get<T>(string key, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(typeInfo);

        string? value = await Get(key, cancellationToken)
                                      .NoSync();

        if (value is null)
            return default;

        if (typeof(T) == typeof(string))
            return (T?) (object) value;

        if (string.IsNullOrWhiteSpace(value))
            return default;

        return JsonSerializer.Deserialize(value, typeInfo);
    }

    [RequiresUnreferencedCode("JSON serialization uses reflection. Use the overload accepting JsonTypeInfo<T> for trimming.")]
    [RequiresDynamicCode("JSON serialization may require runtime code generation. Use the overload accepting JsonTypeInfo<T> for AOT.")]
    public ValueTask Set<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (typeof(T) == typeof(string))
        {
            ArgumentNullException.ThrowIfNull(value);
            return Set(key, (string) (object) value, cancellationToken);
        }

        string json = JsonSerializer.Serialize(value, _serializerOptions);
        return Set(key, json, cancellationToken);
    }

    public ValueTask Set<T>(string key, T value, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(typeInfo);

        if (typeof(T) == typeof(string))
        {
            ArgumentNullException.ThrowIfNull(value);
            return Set(key, (string) (object) value, cancellationToken);
        }

        string json = JsonSerializer.Serialize(value, typeInfo);
        return Set(key, json, cancellationToken);
    }

    private async ValueTask<T> Run<T>(Func<ILibrarianContainer, ValueTask<T>> operation, CancellationToken cancellationToken, bool save = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        // Reopen to read current storage and recover after a conflict. Librarian coordinates writes.
        var database = new SessionStorageLibrarianDatabase(_moduleImportUtil, _logger, "Soenneker.Blazor.Utils.SessionStorage");
        try
        {
            ILibrarianContainer container = await database.GetContainer(_containerName, cancellationToken).NoSync();
            T result = await operation(container).NoSync();
            if (save)
                await database.Save(cancellationToken).NoSync();
            return result;
        }
        finally
        {
            // Never flush or replay an uncertain write during cleanup.
            await database.DiscardAsync().NoSync();
        }
    }
    // Snapshot IDs are case-insensitive. Hex-encoded UTF-16 preserves browser key identity.
    private static string EncodeKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return string.Create(checked(key.Length * 4), key, static (destination, value) =>
        {
            for (var i = 0; i < value.Length; i++)
                ((ushort)value[i]).TryFormat(destination.Slice(i * 4, 4), out _, "X4", CultureInfo.InvariantCulture);
        });
    }

    private static string DecodeKey(string id) => string.Create(id.Length / 4, id, static (destination, value) =>
    {
        for (var i = 0; i < destination.Length; i++)
            destination[i] = (char)ushort.Parse(value.AsSpan(i * 4, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    });

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }
}