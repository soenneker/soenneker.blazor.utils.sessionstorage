[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.utils.sessionstorage)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.SessionStorage

Blazor interop for browser `sessionStorage` operations.

## Install

```bash
dotnet add package Soenneker.Blazor.Utils.SessionStorage
```

## Quick start

```csharp
using Soenneker.Blazor.Utils.SessionStorage.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddSessionStorageUtilAsScoped();
```

Adds `ISessionStorageInterop` and `ISessionStorageUtil` as scoped services.

## What you get

- `ISessionStorageInterop` — Blazor interop for browser `sessionStorage` operations.
- `ISessionStorageUtil` — A higher-level Blazor utility for browser `sessionStorage` built on top of `ISessionStorageInterop`.
- `SessionStorageUtilRegistrar` — Registration for the interop and utility services.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ISessionStorageInterop.Initialize(cancellationToken)` | Ensures the JavaScript module for this package has been loaded and initialized. | A task that completes when the Session Storage is ready for use. |
| `ISessionStorageInterop.Get(key, cancellationToken)` | Gets a stored string value by key, or null if the key does not exist. | A task whose result is the text returned by get. |
| `ISessionStorageInterop.Remove(key, cancellationToken)` | Removes a stored value by key. | A task that completes when the remove operation is complete. |
| `ISessionStorageInterop.Clear(cancellationToken)` | Clears all browser session storage entries. | A task that completes when the Session Storage has been cleared. |
| `ISessionStorageInterop.ContainsKey(key, cancellationToken)` | Returns whether the specified key exists in browser session storage. | true if the specified key exists in the target store; otherwise, false. |
| `ISessionStorageInterop.GetKeys(cancellationToken)` | Returns all session storage keys in index order. | The matching keys as a materialized collection. |
| `ISessionStorageInterop.GetLength(cancellationToken)` | Returns the total number of session storage entries. | A task whose result is the requested value. |
| `ISessionStorageUtil.Initialize(cancellationToken)` | Ensures the underlying JavaScript module has been loaded and is ready for use. | A task that completes when the Session Storage is ready for use. |
| `ISessionStorageUtil.Get(key, cancellationToken)` | Gets a stored string value by key, or null if the key does not exist. | A task whose result is the text returned by get. |
| `ISessionStorageUtil.Remove(key, cancellationToken)` | Removes a stored value by key. | A task that completes when the remove operation is complete. |
| `ISessionStorageUtil.Clear(cancellationToken)` | Clears all browser session storage entries. | A task that completes when the Session Storage has been cleared. |
| `ISessionStorageUtil.ContainsKey(key, cancellationToken)` | Returns whether the specified key exists in browser session storage. | true if the specified key exists in the target store; otherwise, false. |
| `ISessionStorageUtil.GetKeys(cancellationToken)` | Returns all session storage keys in index order. | The matching keys as a materialized collection. |
| `ISessionStorageUtil.GetLength(cancellationToken)` | Returns the total number of session storage entries. | A task whose result is the requested value. |
| `SessionStorageUtilRegistrar.AddSessionStorageUtilAsScoped(services)` | Adds `ISessionStorageInterop` and `ISessionStorageUtil` as scoped services. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
- Dispose instances you own when their scope ends so held resources can be released.
