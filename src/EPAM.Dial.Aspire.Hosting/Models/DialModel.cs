namespace EPAM.Dial.Aspire.Hosting.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a Dial model.
/// </summary>
/// <example>
/// <code>
/// "gpt-4": {
///      "type": "chat",
///      "displayName": "GPT-4",
///      "endpoint": "http://adapter-openai:5000/openai/deployments/gpt-4/chat/completions",
///      "iconUrl": "http://localhost:3001/gpt4.svg",
///      "upstreams": [
///        {
///          "endpoint": "http://azure_deployment_host/openai/deployments/gpt-4/chat/completions",
///          "key": "AZURE_MODEL_API_KEY"
///        }
///      ]
///    }
/// </code>
/// </example>
public class DialModel
{
    private static JsonSerializerOptions JsonSerializerOptions { get; } =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    public string Type { get; set; } = "chat";

    /// <summary>
    /// The name of the model.
    /// </summary>
    public string DisplayName { get; init; } = default!;

    [JsonIgnore]
    public string ModelName { get; set; } = default!;

    public string? Description { get; set; }

    /// <summary>
    /// The model endpoint.
    /// </summary>
    [JsonIgnore]
    public Func<string> EndpointExpression { get; init; } = default!;

    public string Endpoint =>
        this.EndpointExpression?.Invoke() ?? throw new InvalidOperationException("EndpointExpression is not set.");

    /// <summary>
    /// The Icon URL.
    /// </summary>
    public Uri? IconUrl { get; set; }

    public ICollection<ModelUpstream> Upstreams { get; set; } = [];

    internal static string ToJson(IReadOnlyDictionary<string, DialModel> models)
    {
        return JsonSerializer.Serialize(models, JsonSerializerOptions);
    }

    internal static string ToLimitsJson(IReadOnlyDictionary<string, DialModel> models)
    {
        var limits = models.Keys.ToDictionary(model => model, _ => new { });

        return JsonSerializer.Serialize(limits, JsonSerializerOptions);
    }
}
