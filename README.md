[![](https://img.shields.io/nuget/v/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.utils.sessionstorage.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.utils.sessionstorage/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.utils.sessionstorage)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.utils.sessionstorage/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.utils.sessionstorage/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.Utils.SessionStorage
### A Blazor session storage interop and utility library

## Installation

```bash
dotnet add package Soenneker.Blazor.Utils.SessionStorage
```

## Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddSessionStorageUtilAsScoped();
```

Inject the higher-level utility where you need it:

```csharp
@inject ISessionStorageUtil SessionStorage
```

## Usage

Initialize the package once before first use:

```csharp
await SessionStorage.Initialize();
```
