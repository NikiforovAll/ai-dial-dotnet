namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialModelAdapterResource(string name, string modelName, DialResource dial)
    : ContainerResource(name),
        IResourceWithParent<DialResource>,
        IResourceWithConnectionString
{
    public DialResource Parent { get; } = dial;

    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    /// <summary>
    /// Gets the http endpoint for the DIAL Chat resource.
    /// </summary>
    public EndpointReference PrimaryEndpoint => this.primaryEndpoint ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the model name.
    /// </summary>
    public string ModelName { get; } = modelName;

    /// <summary>
    /// Gets the connection string expression for the Ollama model.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.Parent};Model={this.ModelName}");
}
