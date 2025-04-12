namespace EPAM.Dial.Aspire.Hosting.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

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
internal class DialModelDescriptor
{
    public string Type { get; init; } = "chat";

    public string? Endpoint { get; init; }

    /// <summary>
    /// The name of the model.
    /// </summary>
    public string? DisplayName { get; init; } = default!;

    /// <summary>
    /// The model description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Icon URL.
    /// </summary>
    public string? IconUrl { get; set; }

    public IEnumerable<ModelUpstreamDescriptor> Upstreams { get; set; } = [];
}

internal class ModelUpstreamDescriptor
{
    public string Endpoint { get; init; } = default!;
    public string? Key { get; init; }
}

internal static class DialModelDescriptorExtensions
{
    private static JsonSerializerOptions JsonSerializerOptions { get; } =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

    public static string ToJson(this IEnumerable<IDialModelResource> models)
    {
        var modelDict = models.ToDictionary(model => model.DeploymentName, MapFrom);
        return JsonSerializer.Serialize(modelDict, JsonSerializerOptions);
    }

    public static string ToLimitsJson(IReadOnlyDictionary<string, IDialModelResource> models)
    {
        var limits = models.Keys.ToDictionary(model => model, _ => new { });

        return JsonSerializer.Serialize(limits, JsonSerializerOptions);
    }

    private static DialModelDescriptor MapFrom(IDialModelResource model)
    {
        var descriptor = new DialModelDescriptor
        {
            DisplayName = model.DisplayName,
            IconUrl = model.IconUrl ?? MapIcon(model),
            Description = model.Description,
            Endpoint = model switch
            {
                DialModelResource modelResource =>
                    $"{modelResource.Endpoint!.Scheme}://{modelResource.Endpoint.Resource.Name}:{modelResource.Endpoint.TargetPort}/v1/chat/completions",

                DialModelAdapterResource adapterResource =>
                    $"{adapterResource.PrimaryEndpoint.Scheme}://{adapterResource.Name}:{adapterResource.PrimaryEndpoint.TargetPort}/openai/deployments/{model.DeploymentName}/chat/completions",
                _ => throw new NotSupportedException($"Unsupported model type: {model.GetType()}"),
            },
            Upstreams = model switch
            {
                DialModelResource => [],

                DialModelAdapterResource modelAdapterResource =>
                    modelAdapterResource.Upstreams.Select(u => MapUpstreamFrom(model, u)),
                _ => throw new NotSupportedException($"Unsupported model type: {model.GetType()}"),
            },
        };

        return descriptor;
    }

    private static string? MapIcon(IDialModelResource model)
    {
        var themesEndpoint = model.Parent.ChatThemes?.PrimaryEndpoint;

        if (themesEndpoint is null)
        {
            return null;
        }

        var iconName = model.WellKnownIcon switch
        {
            WellKnownIcon.GPT4 => "gpt4.svg",
            WellKnownIcon.GPT3 => "gpt3.svg",
            WellKnownIcon.GPT4Vision => "GPT-4-V.svg",
            WellKnownIcon.GeminiProVision => "Gemini-Pro-Vision.svg",
            WellKnownIcon.Gemini => "Gemini.svg",
            WellKnownIcon.Llama2 => "Llama2.svg",
            WellKnownIcon.Llama3 => "Llama3.svg",
            WellKnownIcon.Anthropic => "anthropic.svg",
            _ => null,
        };

        return $"{themesEndpoint!.Scheme}://{themesEndpoint.Host}:{themesEndpoint.Port}/{iconName}";
    }

    private static ModelUpstreamDescriptor MapUpstreamFrom(
        IDialModelResource model,
        (IResourceWithConnectionString resource, string? key) upstream
    )
    {
        var resource = upstream.resource;

        if (resource.ConnectionStringExpression is null)
        {
            throw new InvalidOperationException(
                $"Resource {resource.Name} does not have a connection string expression."
            );
        }

        var tokens = resource
            .ConnectionStringExpression.GetValueAsync(default)
            .GetAwaiter()
            .GetResult()
            .Split(';');

        var endpoint =
            tokens
                .FirstOrDefault(x => x.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
                ?.Split('=')[1] ?? string.Empty;
        var key =
            upstream.key
            ?? tokens
                .FirstOrDefault(x => x.StartsWith("Key=", StringComparison.OrdinalIgnoreCase))
                ?.Split('=')[1]
            ?? string.Empty;

        var deploymentName = tokens
            .FirstOrDefault(x => x.StartsWith("Deployment=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=')[1];

        return new ModelUpstreamDescriptor
        {
            Endpoint =
                $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName ?? model.DeploymentName}/chat/completions",
            Key = key,
        };
    }
}
