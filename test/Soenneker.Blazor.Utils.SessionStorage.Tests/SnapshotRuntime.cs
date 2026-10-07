using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Soenneker.Blazor.Utils.SessionStorage.Tests;

internal sealed class SnapshotRuntime : IJSRuntime, IJSObjectReference
{
    public string? Snapshot { get; private set; }
    public string? Backend { get; private set; }
    public bool RejectNextWrite { get; set; }
    public int Writes { get; private set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        cancellationToken.ThrowIfCancellationRequested();
        object? result;
        switch (identifier)
        {
            case "import":
                result = this;
                break;
            case "read":
                Backend = (string)args![0]!;
                result = Snapshot;
                break;
            case "compareExchange":
                Writes++;
                bool accepted = !RejectNextWrite && Snapshot == (string?)args![2];
                RejectNextWrite = false;
                if (accepted) Snapshot = (string)args![3]!;
                result = accepted;
                break;
            default:
                throw new InvalidOperationException(identifier);
        }
        return ValueTask.FromResult((TValue)result!);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
