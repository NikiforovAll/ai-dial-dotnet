namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting.Models;

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
        DialModel model
    )
    {
        return builder.AddModelAdapter(name, "epam/ai-dial-adapter-openai:0.22.0", model);
    }

    public static IResourceBuilder<DialModelAdapterResource> AddModelAdapter(
        this IResourceBuilder<DialResource> builder,
        string name,
        string adapter,
        DialModel model
    )
    {
        model.DeploymentName ??= name;
        var modelResource = new DialModelAdapterResource(
            name,
            model.DeploymentName,
            builder.Resource
        );

        var modelBuilder = builder
            .ApplicationBuilder.AddResource(modelResource)
            .WithImage(adapter)
            .WithHttpEndpoint(port: null, targetPort: 5000, DialResource.PrimaryEndpointName)
            .WithEnvironment("WEB_CONCURRENCY", "3");

        var adapterResource = modelBuilder.Resource;
        model.EndpointExpression = () =>
            $"{adapterResource.PrimaryEndpoint.Scheme}://{adapterResource.Name}:{adapterResource.PrimaryEndpoint.TargetPort}/openai/deployments/{model.DeploymentName}/chat/completions";
        builder.Resource.AddModel(model);

        return modelBuilder;
    }

    /// <summary>
    /// Adds a Dial model to the application model. The model is expected to be a OpenAI- compatible local model. E.g.: model deployed via ollama.
    /// </summary>
    public static IResourceBuilder<DialModelResource> AddModel(
        this IResourceBuilder<DialResource> builder,
        string name,
        DialModel model
    )
    {
        model.DeploymentName ??= name;
        var modelResource = new DialModelResource(name, model.DeploymentName, builder.Resource);

        var modelBuilder = builder.ApplicationBuilder.AddResource(modelResource);

        builder.Resource.AddModel(model);

        return modelBuilder;
    }
}
