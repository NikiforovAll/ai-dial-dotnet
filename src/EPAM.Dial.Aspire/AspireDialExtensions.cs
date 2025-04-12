namespace Microsoft.Extensions.Hosting;

using System.ClientModel;
using System.ClientModel.Primitives;
using System.Data.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

/// <summary>
/// Extension methods for setting up Dial client in an <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class AspireDialExtensions
{
    internal const string DefaultConfigSectionName = "Aspire:Dial";

    /// <summary>
    /// Adds <see cref="OpenAIClient"/> services to the container.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to read config from and add services to.</param>
    /// <param name="connectionName">A name used to retrieve the connection string from the ConnectionStrings configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It's invoked after the settings are read from the configuration.</param>
    /// <exception cref="UriFormatException">Thrown when no Dial endpoint is provided.</exception>
    public static AspireDialApiClientBuilder AddDialOpenAIClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<DialSettings>? configureSettings = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentNullException.ThrowIfNull(builder);
        return AddDialClientInternal(
            builder,
            DefaultConfigSectionName,
            connectionName,
            configureSettings: configureSettings
        );
    }

    /// <summary>
    /// Adds <see cref="OpenAIClient"/> services to the container using the <paramref name="connectionName"/> as the service key.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder" /> to read config from and add services to.</param>
    /// <param name="connectionName">A name used to retrieve the connection string from the ConnectionStrings configuration section.</param>
    /// <param name="configureSettings">An optional delegate that can be used for customizing options. It's invoked after the settings are read from the configuration.</param>
    /// <exception cref="UriFormatException">Thrown when no Dial endpoint is provided.</exception>
    public static AspireDialApiClientBuilder AddKeyedDialOpenAIClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<DialSettings>? configureSettings = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentNullException.ThrowIfNull(builder);
        return AddDialClientInternal(
            builder,
            $"{DefaultConfigSectionName}:{connectionName}",
            connectionName,
            serviceKey: connectionName,
            configureSettings: configureSettings
        );
    }

    private static AspireDialApiClientBuilder AddDialClientInternal(
        IHostApplicationBuilder builder,
        string configurationSectionName,
        string connectionName,
        string? serviceKey = null,
        Action<DialSettings>? configureSettings = null
    )
    {
        DialSettings settings = new();
        builder.Configuration.GetSection(configurationSectionName).Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            var connectionBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString,
            };

            if (
                connectionBuilder.ContainsKey("Endpoint")
                && Uri.TryCreate(
                    connectionBuilder["Endpoint"].ToString(),
                    UriKind.Absolute,
                    out var endpoint
                )
            )
            {
                settings.Endpoint = endpoint;
            }

            if (connectionBuilder.ContainsKey("Model"))
            {
                settings.SelectedModel = (string)connectionBuilder["Model"];
            }

            if (connectionBuilder.ContainsKey("Key"))
            {
                settings.Key = (string)connectionBuilder["Key"];
            }
        }

        configureSettings?.Invoke(settings);

        var httpClientKey = $"{connectionName}_httpClient";

        builder.Services.AddHttpClient(
            httpClientKey,
            client =>
            {
                if (settings.Endpoint is not null)
                {
                    client.BaseAddress = settings.Endpoint;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"An DialApiClient could not be configured. Ensure valid connection information was provided in 'ConnectionStrings:{connectionName}' or either "
                            + $"{nameof(settings.Endpoint)} must be provided "
                            + $"in the '{configurationSectionName}' configuration section."
                    );
                }
            }
        );

        if (serviceKey is not null)
        {
            builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => ConfigureDialClient(sp));
        }
        else
        {
            builder.Services.AddSingleton(ConfigureDialClient);

            serviceKey = $"{connectionName}_DialApiClient_internal";
            builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => ConfigureDialClient(sp));
        }

        return new AspireDialApiClientBuilder(builder, serviceKey, settings.SelectedModel);

        OpenAIClient ConfigureDialClient(IServiceProvider serviceProvider)
        {
            var httpClient = serviceProvider
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient(httpClientKey);

            if (!string.IsNullOrWhiteSpace(settings.Key))
            {
                httpClient.DefaultRequestHeaders.Add("Api-Key", settings.Key);
            }

            if (settings.Endpoint is null)
            {
                throw new InvalidOperationException(
                    $"An DialApiClient could not be configured. Ensure valid connection information was provided in 'ConnectionStrings:{connectionName}' or either "
                        + $"{nameof(settings.Endpoint)} must be provided "
                        + $"in the '{configurationSectionName}' configuration section."
                );
            }

            var options = new OpenAIClientOptions()
            {
                Endpoint = new Uri(
                    settings.Endpoint,
                    $"/openai/deployments/{settings.SelectedModel}?api-version=2023-05-15"
                ),
                Transport = new HttpClientPipelineTransport(httpClient),
            };

            var client = new OpenAIClient(new ApiKeyCredential(settings.Key), options);

            return client;
        }
    }
}
