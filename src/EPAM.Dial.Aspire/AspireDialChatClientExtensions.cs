namespace Microsoft.Extensions.Hosting;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

/// <summary>
/// Extension methods for configuring the <see cref="IChatClient" /> from an <see cref="OpenAIClient"/>
/// </summary>
public static class AspireDialChatClientExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="IChatClient"/> in the services provided by the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="AspireDialApiClientBuilder" />.</param>
    /// <returns>A <see cref="ChatClientBuilder"/> that can be used to build a pipeline around the inner <see cref="IChatClient"/>.</returns>
    public static ChatClientBuilder AddChatClient(this AspireDialApiClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.HostBuilder.Services.AddChatClient(services =>
            CreateInnerChatClient(services, builder)
        );
    }

    /// <summary>
    /// Registers a keyed singleton <see cref="IChatClient"/> in the services provided by the <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">An <see cref="AspireDialApiClientBuilder" />.</param>
    /// <returns>A <see cref="ChatClientBuilder"/> that can be used to build a pipeline around the inner <see cref="IChatClient"/>.</returns>
    public static ChatClientBuilder AddKeyedChatClient(this AspireDialApiClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(builder.ServiceKey);

        return builder.HostBuilder.Services.AddKeyedChatClient(
            builder.ServiceKey,
            services => CreateInnerChatClient(services, builder)
        );
    }

    /// <summary>
    /// Wrap the <see cref="OpenAIClient"/> in a telemetry client if tracing is enabled.
    /// Note that this doesn't use ".UseOpenTelemetry()" because the order of the clients would be incorrect.
    /// We want the telemetry client to be the innermost client, right next to the inner <see cref="OpenAIClient"/>.
    /// </summary>
    private static OpenAIChatClient CreateInnerChatClient(
        IServiceProvider services,
        AspireDialApiClientBuilder builder
    )
    {
        if (!string.IsNullOrWhiteSpace(builder.ServiceKey))
        {
            var client = services.GetRequiredKeyedService<OpenAIClient>(builder.ServiceKey);
            return new OpenAIChatClient(client, builder.SelectedModel);
        }

        return new OpenAIChatClient(
            services.GetRequiredService<OpenAIClient>(),
            builder.SelectedModel
        );
    }
}
