using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;

namespace Soenneker.Blazor.Utils.SessionStorage;

/// <inheritdoc cref="ISessionStorageInterop"/>
public sealed class SessionStorageInterop : ISessionStorageInterop
{
    private const string _modulePath = "_content/Soenneker.Blazor.Utils.SessionStorage/js/sessionstorageinterop.js";

    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly CancellationScope _cancellationScope = new();
    private int _disposed;

    public SessionStorageInterop(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil ?? throw new ArgumentNullException(nameof(moduleImportUtil));
    }

    public async ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
        }
    }

    public async ValueTask<string?> Get(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<string?>("get", linked, key);
        }
    }

    public async ValueTask Set(string key, string value, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ArgumentNullException.ThrowIfNull(value);
        ThrowIfDisposed();

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("set", linked, key, value);
        }
    }

    public async ValueTask Remove(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("remove", linked, key);
        }
    }

    public async ValueTask Clear(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("clear", linked);
        }
    }

    public async ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ThrowIfDisposed();

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<bool>("containsKey", linked, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            var keys = await module.InvokeAsync<List<string>>("getKeys", linked);
            return keys ?? [];
        }
    }

    public async ValueTask<int> GetLength(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<int>("getLength", linked);
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Session storage key cannot be null or whitespace.", nameof(key));
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(nameof(SessionStorageInterop));
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _cancellationScope.DisposeAsync();
        await _moduleImportUtil.DisposeContentModule(_modulePath);
    }
}
