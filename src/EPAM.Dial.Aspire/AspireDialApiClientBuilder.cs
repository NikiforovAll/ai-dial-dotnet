namespace Microsoft.Extensions.Hosting;

using OpenAI;

/// <summary>
/// Builder class for configuring and creating an instance of AspireDialApiClient.
/// </summary>
/// <param name="hostBuilder">The <see cref="IHostApplicationBuilder"/> with which services are being registered.</param>
/// <param name="serviceKey">The service key used to register the <see cref="OpenAIClient"/> service, if any.</param>
/// <param name="selectedModel"></param>
public class AspireDialApiClientBuilder(
    IHostApplicationBuilder hostBuilder,
    string serviceKey,
    string selectedModel
)
{
    /// <summary>
    /// The host application builder used to configure the application.
    /// </summary>
    public IHostApplicationBuilder HostBuilder { get; } = hostBuilder;

    /// <summary>
    /// Gets the service key used to register the <see cref="OpenAIClient"/> service, if any.
    /// </summary>
    public string ServiceKey { get; } = serviceKey;

    public string SelectedModel { get; set; } = selectedModel;
}
