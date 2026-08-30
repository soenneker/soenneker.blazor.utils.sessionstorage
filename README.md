[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.utils.sessionstorage)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/codeql.yml)

# Soenneker.Blazor.Utils.SessionStorage

Reads and writes browser `sessionStorage` from Blazor, with string and JSON-serialized typed APIs.

## Install

```bash
dotnet add package Soenneker.Blazor.Utils.SessionStorage
```

Register the scoped services in `Program.cs`:

```csharp
using Soenneker.Blazor.Utils.SessionStorage.Registrars;

builder.Services.AddSessionStorageUtilAsScoped();
```

Inject the higher-level utility into a component or service:

```razor
@using Soenneker.Blazor.Utils.SessionStorage.Abstract
@inject ISessionStorageUtil SessionStorage
```

## Strings

```csharp
await SessionStorage.Set("checkout.step", "shipping", cancellationToken);

string? step = await SessionStorage.Get("checkout.step", cancellationToken);
```

`Get` returns `null` when the key does not exist. Keys must contain at least one non-whitespace character, and string values cannot be null.

## Typed values

The generic overloads serialize values with `System.Text.Json` web defaults:

```csharp
public sealed record CheckoutDraft(string Email, int ItemCount);

var draft = new CheckoutDraft("buyer@example.com", 3);
await SessionStorage.Set("checkout.draft", draft, cancellationToken);

CheckoutDraft? restored =
    await SessionStorage.Get<CheckoutDraft>("checkout.draft", cancellationToken);
```

A missing key, stored JSON `null`, or an empty/whitespace stored value returns `default(T)`. Malformed JSON or a value incompatible with `T` throws `JsonException`. Generic string calls use the raw string representation rather than JSON quoting.

## Inspect and remove entries

```csharp
bool exists = await SessionStorage.ContainsKey("checkout.draft", cancellationToken);
int count = await SessionStorage.GetLength(cancellationToken);
IReadOnlyList<string> keys = await SessionStorage.GetKeys(cancellationToken);

await SessionStorage.Remove("checkout.draft", cancellationToken);
```

`GetKeys` follows the browser's storage index order; do not treat it as sorted.

`Clear` removes every `sessionStorage` entry for the current origin, including entries created by other code:

```csharp
await SessionStorage.Clear(cancellationToken);
```

`Initialize` is optional. Normal operations import the JavaScript module on demand; call it during component initialization only when you want to pay that cost ahead of the first storage operation.

## Browser behavior and security

- `sessionStorage` is scoped to the current origin and top-level browser tab. It survives reloads but is cleared when the tab's page session ends.
- Browser privacy settings, storage policy, or quota limits can make an operation fail; the browser exception is propagated to the caller.
- Storage is unavailable during server prerendering. Invoke this utility only after the component can perform JavaScript interop.
- Values are not encrypted. Any script running on the origin can read them, so do not store access tokens, passwords, or other secrets here.
- Cancellation stops the .NET wait but cannot undo a browser storage mutation that already completed.

## Low-level interop

`ISessionStorageInterop` exposes the raw string operations used by the utility. Most consumers should use `ISessionStorageUtil`. Both are scoped, and dependency injection disposes the interop and its imported module automatically.
