namespace EPAM.Dial.Aspire.Hosting.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

internal static class DescriptorExtensions
{
    private static JsonSerializerOptions JsonSerializerOptions { get; } =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

    public static string ToJson<T>(T source)
    {
        return JsonSerializer.Serialize(source, JsonSerializerOptions);
    }

    public static DialDeploymentDescriptor ToDescriptor(this DialResource dial)
    {
        var models = MapModels(dial);
        var applications = MapApplications(dial);
        var assistantDescriptor = MapAssistant(dial.Assistant);
        var addons = MapAddons(dial);
        var limits = models
            .Keys.Concat(assistantDescriptor?.Assistants.Keys.Select(x => x) ?? [])
            .Concat(addons.Keys)
            .Concat(applications.Keys)
            .ToDictionary(model => model, _ => new { });

        var descriptor = new DialDeploymentDescriptor
        {
            Models = models,
            Applications = applications,
            Assistant = assistantDescriptor,
            Addons = addons,
            Roles = new() { ["default"] = new Dictionary<string, object?> { ["limits"] = limits } },
            Keys = new Dictionary<string, object>
            {
                ["dial_api_key"] = new Dictionary<string, string>
                {
                    ["project"] = "TEST-PROJECT",
                    ["role"] = "default",
                },
            },
        };

        return descriptor;
    }

    private static Dictionary<string, ApplicationDescriptor> MapApplications(DialResource dial)
    {
        var applications = dial.Applications.Values.ToDictionary(x => x.Name, ToDescriptor);
        return applications;
    }

    public static AssistantDescriptor ToDescriptor(this DialAssistantResource assistant)
    {
        var descriptor = new AssistantDescriptor
        {
            Prompt = assistant.Prompt,
            DisplayName = assistant.DisplayName,
            Description = assistant.Description,
            Addons = assistant.Addons?.Select(a => a.Name)?.ToList() ?? [],
        };

        return descriptor;
    }

    public static ModelDescriptor ToDescriptor(this IDialModelResource model)
    {
        var descriptor = new ModelDescriptor
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

    public static AddonDescriptor ToDescriptor(this DialAddonResource addon)
    {
        var endpointReference =
            addon.PrimaryEndpoint
            ?? throw new ArgumentException($"Addon {addon.Name} does not have a primary endpoint.");

        var endpoint = GetEndpoint(endpointReference, addon.Parent.PrimaryEndpoint.ContainerHost);
        return new()
        {
            Endpoint = $"{endpoint}/{addon.Path?.TrimStart('/')}",
            DisplayName = addon.DisplayName,
            Description = addon.Description,
        };
    }

    public static ApplicationDescriptor ToDescriptor(this DialApplicationResource application)
    {
        var endpointReference =
            application.PrimaryEndpoint
            ?? throw new ArgumentException(
                $"Addon {application.Name} does not have a primary endpoint."
            );

        var endpoint = GetEndpoint(endpointReference, application.Parent.PrimaryEndpoint.ContainerHost);
        return new()
        {
            Endpoint = $"{endpoint}/chat/completions",
            DisplayName = application.DisplayName,
            Description = application.Description,
        };
    }

    private static string GetEndpoint(EndpointReference endpoint, string containerHostName)
    {
        var isContainer = endpoint.Resource.IsContainer();

        var hostname = isContainer ? endpoint.Resource.Name : containerHostName;
        var port = isContainer ? endpoint.TargetPort : endpoint.Port;

        return $"{endpoint.Scheme}://{hostname}:{port}";
    }

    private static Dictionary<string, AddonDescriptor> MapAddons(DialResource dial)
    {
        var addons = dial.Addons.Values.ToDictionary(x => x.Name, ToDescriptor);

        return addons;
    }

    private static AssistantBuilderDescriptor? MapAssistant(DialAssistantBuilderResource? assistant)
    {
        if (assistant is null)
        {
            return null;
        }

        var assistants = assistant.Assistants.ToDictionary(x => x.Name, ToDescriptor);

        var descriptor = new AssistantBuilderDescriptor
        {
            Endpoint =
                $"{assistant.PrimaryEndpoint.Scheme}://{assistant.Name}:{assistant.PrimaryEndpoint.TargetPort}/openai/deployments/{assistant.Name}/chat/completions",
            Assistants = assistants,
        };

        return descriptor;
    }

    private static Dictionary<string, ModelDescriptor> MapModels(DialResource dial)
    {
        var models = dial.Models.Values.ToDictionary(model => model.DeploymentName, ToDescriptor);

        return models;
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
