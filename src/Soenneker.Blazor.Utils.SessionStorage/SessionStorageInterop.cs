using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Librarian.Abstractions;
using Soenneker.Librarian.SessionStorage;

namespace Soenneker.Blazor.Utils.SessionStorage;

public sealed class SessionStorageInterop : ISessionStorageInterop
{
    private const string _containerName = "entries";
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly ILogger<SessionStorageLibrarianDatabase> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public SessionStorageInterop(IModuleImportUtil moduleImportUtil, ILogger<SessionStorageLibrarianDatabase> logger)
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

    private async ValueTask<T> Run<T>(Func<ILibrarianContainer, ValueTask<T>> operation, CancellationToken cancellationToken, bool save = false)
    {
        await _gate.WaitAsync(cancellationToken).NoSync();
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            // Reopen each operation to read the current snapshot and recover naturally after a conflict.
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
        finally
        {
            _gate.Release();
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

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync().NoSync();
        try { _disposed = true; }
        finally { _gate.Release(); }
    }
}
