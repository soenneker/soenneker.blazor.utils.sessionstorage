using System;
using Soenneker.Blazor.Utils.ModuleImport;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Librarian.Abstractions;
using Soenneker.Librarian.SessionStorage;
using System.Threading;

namespace Soenneker.Blazor.Utils.SessionStorage.Tests;

public sealed class LibrarianStorageTests
{
    [Test]
    public async Task PersistsDocumentsAndPreservesCaseSensitiveKeys(CancellationToken cancellationToken)
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var first = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await first.Set("Key", "plain text", cancellationToken: cancellationToken);
        await first.Set("key", "{\"number\":42}", cancellationToken: cancellationToken);
        await first.Set("emoji-😀", "", cancellationToken: cancellationToken);
        if (runtime.Backend != "sessionStorage" || runtime.Snapshot is null)
            throw new InvalidOperationException("The Librarian browser backend was not used.");

        await using var second = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        if (await second.Get("Key", cancellationToken: cancellationToken) != "plain text" || await second.Get("key", cancellationToken: cancellationToken) != "{\"number\":42}")
            throw new InvalidOperationException("Stored documents did not survive reopening.");
        if (!(await second.GetKeys(cancellationToken: cancellationToken)).Order().SequenceEqual(new[] { "Key", "key", "emoji-😀" }.Order()))
            throw new InvalidOperationException("Key identities were not preserved.");
        await second.Remove("key", cancellationToken: cancellationToken);
        if (await first.ContainsKey("key", cancellationToken: cancellationToken) || await first.GetLength(cancellationToken: cancellationToken) != 2 || !await first.ContainsKey("emoji-😀", cancellationToken: cancellationToken))
            throw new InvalidOperationException("Reads must see persisted mutations from other owners.");
        await second.Clear(cancellationToken: cancellationToken);
        if (await first.GetLength(cancellationToken: cancellationToken) != 0)
            throw new InvalidOperationException("Clear did not persist.");
    }

    [Test]
    public async Task ConflictDoesNotPublishOrReplayFailedWrite(CancellationToken cancellationToken)
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var storage = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await storage.Set("key", "original", cancellationToken: cancellationToken);
        runtime.RejectNextWrite = true;
        try
        {
            await storage.Set("key", "rejected", cancellationToken: cancellationToken);
            throw new InvalidOperationException("Expected a concurrency exception.");
        }
        catch (LibrarianConcurrencyException) { }
        if (runtime.Writes != 2 || await storage.Get("key", cancellationToken: cancellationToken) != "original")
            throw new InvalidOperationException("A failed write was published or replayed.");
        await storage.Set("key", "recovered", cancellationToken: cancellationToken);
        if (await storage.Get("key", cancellationToken: cancellationToken) != "recovered")
            throw new InvalidOperationException("The next operation did not reopen the database.");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task FailedDeletePreservesPersistedData(bool clear, CancellationToken cancellationToken)
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var storage = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await storage.Set("key", "original", cancellationToken: cancellationToken);
        runtime.RejectNextWrite = true;
        try
        {
            if (clear)
                await storage.Clear(cancellationToken: cancellationToken);
            else
                await storage.Remove("key", cancellationToken: cancellationToken);
            throw new InvalidOperationException("Expected a concurrency exception.");
        }
        catch (LibrarianConcurrencyException) { }
        if (runtime.Writes != 2 || await storage.Get("key", cancellationToken: cancellationToken) != "original")
            throw new InvalidOperationException("A failed delete was persisted or replayed.");
        if (clear)
            await storage.Clear(cancellationToken: cancellationToken);
        else
            await storage.Remove("key", cancellationToken: cancellationToken);
        if (await storage.ContainsKey("key", cancellationToken: cancellationToken))
            throw new InvalidOperationException("The next delete did not recover.");
    }

    [Test]
    public async Task OverlappingWritesUseLibrarianConflictDetection(CancellationToken cancellationToken)
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var storage = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await storage.Set("key", "original", cancellationToken: cancellationToken);

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        runtime.WriteStarted = started;
        runtime.ResumeWrite = resume;
        Task pending = storage.Set("key", "pending", cancellationToken: cancellationToken).AsTask();
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken: cancellationToken);
            await storage.Set("key", "winner", cancellationToken: cancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), cancellationToken: cancellationToken);
        }
        finally
        {
            resume.TrySetResult();
            try
            {
                await pending;
                throw new InvalidOperationException("Expected Librarian to reject the stale snapshot.");
            }
            catch (LibrarianConcurrencyException) { }
        }

        if (await storage.Get("key", cancellationToken: cancellationToken) != "winner")
            throw new InvalidOperationException("The stale write overwrote the committed value.");

        await storage.DisposeAsync();
        try
        {
            await storage.Get("key", cancellationToken: cancellationToken);
            throw new InvalidOperationException("Disposed utilities must reject new operations.");
        }
        catch (ObjectDisposedException) { }
    }
}
