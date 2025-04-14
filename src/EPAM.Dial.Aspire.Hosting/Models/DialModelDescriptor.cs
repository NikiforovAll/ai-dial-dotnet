namespace EPAM.Dial.Aspire.Hosting.Models;

public class DialDeploymentDescriptor
{
    public object? Routes { get; set; } = new { };

    public AssistantBuilderDescriptor? Assistant { get; set; }

    public Dictionary<string, ModelDescriptor>? Models { get; set; }

    public Dictionary<string, ApplicationDescriptor>? Applications { get; set; }

    public Dictionary<string, AddonDescriptor>? Addons { get; set; }

    public Dictionary<string, object>? Keys { get; set; }

    public Dictionary<string, object>? Roles { get; set; }
}

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
public class ModelDescriptor
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

public class ModelUpstreamDescriptor
{
    public string Endpoint { get; init; } = default!;
    public string? Key { get; init; }
}

public class AssistantBuilderDescriptor
{
    public string? Endpoint { get; init; }

    public Dictionary<string, AssistantDescriptor> Assistants { get; set; } = [];
}

public class AssistantDescriptor
{
    public string Prompt { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public List<string>? Addons { get; set; } = [];
}

public class AddonDescriptor
{
    public string? Endpoint { get; init; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}

public class ApplicationDescriptor
{
    public string? Endpoint { get; init; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}
