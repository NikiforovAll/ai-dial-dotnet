namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting;
using EPAM.Dial.Aspire.Hosting.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DialBuilderAssistantExtensions
{
    public static IResourceBuilder<DialAssistantBuilderResource> AddAssistantsBuilder(
        this IResourceBuilder<DialResource> builder,
        string name = "assistant"
    )
    {
        var assistant = new DialAssistantBuilderResource(name, builder.Resource);

        var modelBuilder = builder
            .ApplicationBuilder.AddResource(assistant)
            .WithImage(
                DialCoreContainerImageTags.AssistantImage,
                DialCoreContainerImageTags.AssistantTag
            )
            .WithImageRegistry(DialCoreContainerImageTags.AssistantRegistry)
            .WithHttpEndpoint(port: null, targetPort: 5000, DialResource.PrimaryEndpointName)
            .WithEnvironment("WEB_CONCURRENCY", "3")
            .WithEnvironment("LOG_LEVEL", "DEBUG")
            .WithEnvironment(context =>
                context.EnvironmentVariables.Add(
                    "OPENAI_API_BASE",
                    $"{builder.Resource.PrimaryEndpoint.Scheme}://{builder.Resource.Name}:{builder.Resource.PrimaryEndpoint.TargetPort}"
                )
            );

        builder.Resource.Assistant = assistant;

        return modelBuilder;
    }

    public static IResourceBuilder<DialAssistantResource> AddAssistant(
        this IResourceBuilder<DialAssistantBuilderResource> builder,
        string name
    )
    {
        var assistant = new DialAssistantResource(name, builder.Resource);
        var assistantBuilder = builder.ApplicationBuilder.AddResource(assistant);
        builder.Resource.Assistants.Add(assistant);

        assistantBuilder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            assistantBuilder.Resource,
            (@event, cancellationToken) =>
            {
                var logger = @event
                    .Services.GetRequiredService<ResourceLoggerService>()
                    .GetLogger(assistantBuilder.Resource);

                logger.LogInformation(
                    "Starting assistant - '{AssistantName}'",
                    assistantBuilder.Resource.Name
                );
                logger.LogInformation(
                    "{AssistantConfig}",
                    DescriptorExtensions.ToJson(assistantBuilder.Resource.ToDescriptor())
                );

                return Task.CompletedTask;
            }
        );

        return assistantBuilder;
    }

    public static IResourceBuilder<DialAssistantResource> WithPrompt(
        this IResourceBuilder<DialAssistantResource> builder,
        string prompt
    )
    {
        builder.Resource.Prompt = prompt;
        return builder;
    }

    public static IResourceBuilder<DialAssistantResource> WithDisplayName(
        this IResourceBuilder<DialAssistantResource> builder,
        string? displayName
    )
    {
        builder.Resource.DisplayName = displayName;
        return builder;
    }

    public static IResourceBuilder<DialAssistantResource> WithDescription(
        this IResourceBuilder<DialAssistantResource> builder,
        string? description
    )
    {
        builder.Resource.Description = description;
        return builder;
    }

    public static IResourceBuilder<DialAssistantResource> WithAddon(
        this IResourceBuilder<DialAssistantResource> builder,
        IResourceBuilder<DialAddonResource> addonBuilder
    )
    {
        builder.Resource.Addons.Add(addonBuilder.Resource);
        builder.WithRelationship(addonBuilder.Resource, "consumes");
        return builder;
    }

    public static IResourceBuilder<DialAddonResource> AddAddon(
        this IResourceBuilder<DialResource> builder,
        string name
    )
    {
        var addon = new DialAddonResource(name, builder.Resource);

        var addonBuilder = builder.ApplicationBuilder.AddResource(addon);
        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            addonBuilder.Resource,
            (@event, cancellationToken) =>
            {
                var logger = @event
                    .Services.GetRequiredService<ResourceLoggerService>()
                    .GetLogger(addonBuilder.Resource);

                logger.LogInformation("Adding addon - '{AddonName}'", addonBuilder.Resource.Name);
                logger.LogInformation(
                    "{Config}",
                    DescriptorExtensions.ToJson(addonBuilder.Resource.ToDescriptor())
                );

                return Task.CompletedTask;
            }
        );

        builder.Resource.AddAddon(addon);

        return addonBuilder;
    }

    public static IResourceBuilder<DialAddonResource> WithUpstream(
        this IResourceBuilder<DialAddonResource> builder,
        IResourceBuilder<IResourceWithEndpoints> resource,
        string path = "/.well-known/ai-plugin.json",
        string endpointName = "http"
    )
    {
        builder.Resource.PrimaryEndpoint = resource.Resource.GetEndpoint(endpointName);
        builder.Resource.Path = path;

        resource.WithReferenceRelationship(builder.Resource.Parent);

        return builder;
    }

    public static IResourceBuilder<DialAddonResource> WithDisplayName(
        this IResourceBuilder<DialAddonResource> builder,
        string? displayName
    )
    {
        builder.Resource.DisplayName = displayName;
        return builder;
    }

    public static IResourceBuilder<DialAddonResource> WithDescription(
        this IResourceBuilder<DialAddonResource> builder,
        string? description
    )
    {
        builder.Resource.Description = description;
        return builder;
    }
}
