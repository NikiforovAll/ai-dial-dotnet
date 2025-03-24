namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Represents the settings for OllamaSharp.
/// </summary>
public sealed class DialSettings
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public Uri? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the selected model.
    /// </summary>
    public string SelectedModel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the list of models to available.
    /// </summary>
    public IReadOnlyList<string> Models { get; set; } = [];

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the Ollama health check is disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableHealthChecks { get; set; }

    /// <summary>
    /// Gets or sets a integer value that indicates the Ollama health check timeout in milliseconds.
    /// </summary>
    public int? HealthCheckTimeout { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether tracing is disabled or not.
    /// </summary>
    internal bool DisableTracing { get; set; }
}
