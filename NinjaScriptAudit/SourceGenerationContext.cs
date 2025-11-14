using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NinjaScriptAudit {
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
    [JsonSerializable(typeof(OAuthTokenResponse))]
    [JsonSerializable(typeof(Script))]
    [JsonSerializable(typeof(ScriptMetadata))]
    [JsonSerializable(typeof(List<ScriptMetadata>))]
    internal partial class SourceGenerationContext : JsonSerializerContext {
    }
}
