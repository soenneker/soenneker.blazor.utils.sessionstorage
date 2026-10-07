using System;
using Soenneker.Blazor.Utils.ModuleImport;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Soenneker.Librarian.Abstractions;
using Soenneker.Librarian.SessionStorage;

namespace Soenneker.Blazor.Utils.SessionStorage.Tests;

public sealed class LibrarianStorageTests
{
    [Test]
    public async Task PersistsDocumentsAndPreservesCaseSensitiveKeys()
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var first = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await first.Set("Key", "plain text");
        await first.Set("key", "{\"number\":42}");
        await first.Set("emoji-😀", "");
        if (runtime.Backend != "sessionStorage" || runtime.Snapshot is null)
            throw new InvalidOperationException("The Librarian browser backend was not used.");

        await using var second = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        if (await second.Get("Key") != "plain text" || await second.Get("key") != "{\"number\":42}")
            throw new InvalidOperationException("Stored documents did not survive reopening.");
        if (!(await second.GetKeys()).Order().SequenceEqual(new[] { "Key", "key", "emoji-😀" }.Order()))
            throw new InvalidOperationException("Key identities were not preserved.");
        await second.Remove("key");
        if (await first.ContainsKey("key") || await first.GetLength() != 2 || !await first.ContainsKey("emoji-😀"))
            throw new InvalidOperationException("Reads must see persisted mutations from other owners.");
        await second.Clear();
        if (await first.GetLength() != 0)
            throw new InvalidOperationException("Clear did not persist.");
    }

    [Test]
    public async Task ConflictDoesNotPublishOrReplayFailedWrite()
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var storage = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await storage.Set("key", "original");
        runtime.RejectNextWrite = true;
        try
        {
            await storage.Set("key", "rejected");
            throw new InvalidOperationException("Expected a concurrency exception.");
        }
        catch (LibrarianConcurrencyException) { }
        if (runtime.Writes != 2 || await storage.Get("key") != "original")
            throw new InvalidOperationException("A failed write was published or replayed.");
        await storage.Set("key", "recovered");
        if (await storage.Get("key") != "recovered")
            throw new InvalidOperationException("The next operation did not reopen the database.");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task FailedDeletePreservesPersistedData(bool clear)
    {
        var runtime = new SnapshotRuntime();
        await using var modules = new ModuleImportUtil(runtime);
        await using var storage = new SessionStorageUtil(modules, NullLogger<SessionStorageLibrarianDatabase>.Instance);
        await storage.Set("key", "original");
        runtime.RejectNextWrite = true;
        try
        {
            if (clear)
                await storage.Clear();
            else
                await storage.Remove("key");
            throw new InvalidOperationException("Expected a concurrency exception.");
        }
        catch (LibrarianConcurrencyException) { }
        if (runtime.Writes != 2 || await storage.Get("key") != "original")
            throw new InvalidOperationException("A failed delete was persisted or replayed.");
        if (clear)
            await storage.Clear();
        else
            await storage.Remove("key");
        if (await storage.ContainsKey("key"))
            throw new InvalidOperationException("The next delete did not recover.");
    }
}
