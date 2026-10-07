using System.Text.Json.Serialization;

namespace Soenneker.Blazor.Utils.SessionStorage;

[JsonSerializable(typeof(StorageDocument))]
internal partial class StorageJsonContext : JsonSerializerContext;
