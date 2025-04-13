namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting;
using EPAM.Dial.Aspire.Hosting.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides extension methods for adding Dial to the application model.
/// </summary>
public static class DialBuilderModelExtensions
{
    /// <summary>
    /// Adds a Dial model adapter to the application model. OpenAI model adapter configured to proxy to OpenAI-compatible service.
    /// </summary>
    public static IResourceBuilder<DialModelAdapterResource> AddOpenAIModelAdapter(
        this IResourceBuilder<DialResource> builder,
        string name,
        string deploymentName
    )
    {
        return builder.AddModelAdapter(
            name,
            deploymentName,
            $"{DialImageTags.OpenAIAdapterImage}:{DialImageTags.OpenAIAdapterTag}"
        );
    }

    public static IResourceBuilder<DialModelAdapterResource> AddModelAdapter(
        this IResourceBuilder<DialResource> builder,
        string name,
        string deploymentName,
        string adapter
    )
    {
        var modelResource = new DialModelAdapterResource(name, deploymentName, builder.Resource);

        var modelBuilder = builder
            .ApplicationBuilder.AddResource(modelResource)
            .WithImage(adapter)
            .WithHttpEndpoint(port: null, targetPort: 5000, DialResource.PrimaryEndpointName)
            .WithEnvironment("WEB_CONCURRENCY", "3");

        modelBuilder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            modelBuilder.Resource,
            (@event, cancellationToken) =>
            {
                var logger = @event
                    .Services.GetRequiredService<ResourceLoggerService>()
                    .GetLogger(modelBuilder.Resource);

                logger.LogInformation("Starting model - '{ModelName}'", modelBuilder.Resource.Name);
                logger.LogInformation(
                    "{Config}",
                    DescriptorExtensions.ToJson(modelBuilder.Resource.ToDescriptor())
                );

                return Task.CompletedTask;
            }
        );

        modelResource.Endpoint = modelBuilder.Resource.PrimaryEndpoint;
        builder.Resource.AddModel(modelResource);

        return modelBuilder;
    }

    public static IResourceBuilder<DialModelAdapterResource> WithUpstream(
        this IResourceBuilder<DialModelAdapterResource> builder,
        IResourceBuilder<IResourceWithConnectionString> resourceBuilder,
        IResourceBuilder<ParameterResource>? key = null
    )
    {
        builder.Resource.Upstreams.Add((resourceBuilder.Resource, key?.Resource?.Value));
        return builder;
    }

    public static IResourceBuilder<DialModelResource> AddModel(
        this IResourceBuilder<DialResource> builder,
        string name,
        string deploymentName
    )
    {
        var modelResource = new DialModelResource(name, deploymentName, builder.Resource);

        var modelBuilder = builder.ApplicationBuilder.AddResource(modelResource);

        modelBuilder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            modelBuilder.Resource,
            (@event, cancellationToken) =>
            {
                var logger = @event
                    .Services.GetRequiredService<ResourceLoggerService>()
                    .GetLogger(modelBuilder.Resource);

                logger.LogInformation("Starting model - '{ModelName}'", modelBuilder.Resource.Name);
                logger.LogInformation(
                    "{Config}",
                    DescriptorExtensions.ToJson(modelBuilder.Resource.ToDescriptor())
                );

                return Task.CompletedTask;
            }
        );

        builder.Resource.AddModel(modelResource);

        return modelBuilder;
    }

    public static IResourceBuilder<DialModelResource> WithEndpoint(
        this IResourceBuilder<DialModelResource> builder,
        EndpointReference endpoint
    )
    {
        builder.Resource.Endpoint = endpoint;
        return builder;
    }

    public static IResourceBuilder<IDialModelResource> WithDisplayName(
        this IResourceBuilder<IDialModelResource> builder,
        string displayName
    )
    {
        builder.Resource.DisplayName = displayName;
        return builder;
    }

    public static IResourceBuilder<IDialModelResource> WithDescription(
        this IResourceBuilder<IDialModelResource> builder,
        string description
    )
    {
        builder.Resource.Description = description;
        return builder;
    }

    public static IResourceBuilder<IDialModelResource> WithIconUrl(
        this IResourceBuilder<IDialModelResource> builder,
        string iconUrl
    )
    {
        builder.Resource.IconUrl = iconUrl;
        return builder;
    }

    public static IResourceBuilder<IDialModelResource> WithWellKnownIcon(
        this IResourceBuilder<IDialModelResource> builder,
        WellKnownIcon icon
    )
    {
        builder.Resource.WellKnownIcon = icon;
        return builder;
    }
}
