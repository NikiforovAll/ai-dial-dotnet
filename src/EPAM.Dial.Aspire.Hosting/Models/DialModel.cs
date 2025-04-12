namespace EPAM.Dial.Aspire.Hosting.Models;

using System.Text.Json.Serialization;

[Obsolete("Use DialModelResource instead.")]
public class DialModel
{
    /// <summary>
    /// The model endpoint.
    /// </summary>
    [JsonIgnore]
    public Func<string> EndpointExpression { get; set; } = default!;

    public string Endpoint =>
        this.EndpointExpression?.Invoke()
        ?? throw new InvalidOperationException("EndpointExpression is not set.");

    /// <summary>
    /// The name of the model.
    /// </summary>
    public string DisplayName { get; init; } = default!;

    public string DeploymentName { get; set; } = default!;

    public string? Description { get; set; }

    /// <summary>
    /// The Icon URL.
    /// </summary>
    public Uri? IconUrl { get; set; }

    [JsonIgnore]
    public ICollection<Func<ModelUpstream>> Upstreams { get; init; } = [];

    [JsonPropertyName("upstreams")]
    public ICollection<ModelUpstream> UpstreamsValue { get; set; } = [];

    public DialModel WithUpstream(ModelUpstream upstream)
    {
        this.Upstreams.Add(() => upstream);
        return this;
    }
}

public class ModelUpstream
{
    public string Endpoint { get; init; } = default!;
    public string Key { get; init; } = default!;
}
