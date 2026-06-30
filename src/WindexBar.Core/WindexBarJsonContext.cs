using System.Text.Json.Serialization;
using WindexBar.Core.Config;
using WindexBar.Core.Providers.Codex;
using WindexBar.Core.Refresh;

namespace WindexBar.Core;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(WindexBarConfig))]
[JsonSerializable(typeof(RpcRateLimitsResponse))]
[JsonSerializable(typeof(RpcAccountResponse))]
[JsonSerializable(typeof(RateLimitResetCreditState))]
internal sealed partial class WindexBarJsonContext : JsonSerializerContext;
