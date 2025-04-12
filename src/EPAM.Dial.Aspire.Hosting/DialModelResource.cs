namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialModelResource(string name, string deploymentName, DialResource dial)
    : Resource(name),
        IDialModelResource
{
    /// <summary>
    /// Gets the connection string expression for the Ollama model.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.Parent};Model={this.DeploymentName}");

    public DialResource Parent { get; } = dial;

    public string DeploymentName { get; } = deploymentName;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public WellKnownIcon? WellKnownIcon { get; set; }

    public EndpointReference? Endpoint { get; set; }
}
