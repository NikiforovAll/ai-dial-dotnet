namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DialBuilderApplicationExtensions
{
    public static IResourceBuilder<DialApplicationResource> AddApplication(
        this IResourceBuilder<DialResource> builder,
        string name
    )
    {
        var application = new DialApplicationResource(name, builder.Resource);
        var applicationBuilder = builder.ApplicationBuilder.AddResource(application);
        builder.Resource.AddApplication(application);

        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            applicationBuilder.Resource,
            (@event, cancellationToken) =>
            {
                var logger = @event
                    .Services.GetRequiredService<ResourceLoggerService>()
                    .GetLogger(applicationBuilder.Resource);

                logger.LogInformation(
                    "Starting application - '{Name}'",
                    applicationBuilder.Resource.Name
                );
                logger.LogInformation(
                    "{Config}",
                    DescriptorExtensions.ToJson(applicationBuilder.Resource.ToDescriptor())
                );

                return Task.CompletedTask;
            }
        );

        return applicationBuilder;
    }

    public static IResourceBuilder<DialApplicationResource> WithUpstream(
        this IResourceBuilder<DialApplicationResource> builder,
        IResourceBuilder<IResourceWithEndpoints> resource,
        string endpointName = "http"
    )
    {
        builder.Resource.PrimaryEndpoint = resource.Resource.GetEndpoint(endpointName);

        builder.WithRelationship(resource.Resource, "consumes");

        return builder;
    }

    public static IResourceBuilder<DialApplicationResource> WithDisplayName(
        this IResourceBuilder<DialApplicationResource> builder,
        string? displayName
    )
    {
        builder.Resource.DisplayName = displayName;
        return builder;
    }

    public static IResourceBuilder<DialApplicationResource> WithDescription(
        this IResourceBuilder<DialApplicationResource> builder,
        string? description
    )
    {
        builder.Resource.Description = description;
        return builder;
    }
}
