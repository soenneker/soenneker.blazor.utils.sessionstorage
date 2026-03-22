using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.Utils.SessionStorage.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;

namespace Soenneker.Blazor.Utils.SessionStorage;

/// <inheritdoc cref="ISessionStorageInterop"/>
public sealed class SessionStorageInterop : ISessionStorageInterop
{
    private const string _modulePath = "Soenneker.Blazor.Utils.SessionStorage/js/sessionstorageinterop.js";
    private const string _jsInitialize = "SessionStorageInterop.initialize";
    private const string _jsGet = "SessionStorageInterop.get";
    private const string _jsSet = "SessionStorageInterop.set";
    private const string _jsRemove = "SessionStorageInterop.remove";
    private const string _jsClear = "SessionStorageInterop.clear";
    private const string _jsContainsKey = "SessionStorageInterop.containsKey";
    private const string _jsGetKeys = "SessionStorageInterop.getKeys";
    private const string _jsGetLength = "SessionStorageInterop.getLength";

    private readonly IJSRuntime _jsRuntime;
    private readonly IResourceLoader _resourceLoader;
    private readonly AsyncInitializer _initializer;
    private readonly CancellationScope _cancellationScope = new();

    private bool _disposed;

    public SessionStorageInterop(IJSRuntime jsRuntime, IResourceLoader resourceLoader)
    {
        _jsRuntime = jsRuntime;
        _resourceLoader = resourceLoader;
        _initializer = new AsyncInitializer(InitializeModule);
    }

    private async ValueTask InitializeModule(CancellationToken cancellationToken)
    {
        _ = await _resourceLoader.ImportModule(_modulePath, cancellationToken);
    }

    private async ValueTask EnsureInitialized(CancellationToken cancellationToken)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await _initializer.Init(linked);
        }
    }

    public async ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsInitialize, linked);
        }
    }

    public async ValueTask<string?> Get(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<string?>(_jsGet, linked, key);
        }
    }

    public async ValueTask Set(string key, string value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsSet, linked, key, value ?? "");
        }
    }

    public async ValueTask Remove(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsRemove, linked, key);
        }
    }

    public async ValueTask Clear(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsClear, linked);
        }
    }

    public async ValueTask<bool> ContainsKey(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<bool>(_jsContainsKey, linked, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetKeys(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            var keys = await _jsRuntime.InvokeAsync<List<string>>(_jsGetKeys, linked);
            return keys ?? [];
        }
    }

    public async ValueTask<int> GetLength(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<int>(_jsGetLength, linked);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _resourceLoader.DisposeModule(_modulePath);
        await _initializer.DisposeAsync();
        await _cancellationScope.DisposeAsync();
    }
}
