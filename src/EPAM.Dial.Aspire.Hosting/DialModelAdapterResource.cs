namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialModelAdapterResource(string name, string deploymentName, DialResource dial)
    : ContainerResource(name),
        IDialModelResource
{
    public DialResource Parent { get; } = dial;

    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    public EndpointReference PrimaryEndpoint =>
        this.primaryEndpoint ??= new(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.Parent};Model={this.DeploymentName}");

    public string DeploymentName { get; } = deploymentName;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public WellKnownIcon? WellKnownIcon { get; set; }

    public EndpointReference? Endpoint { get; set; }

    public IList<(IResourceWithConnectionString resource, string? key)> Upstreams { get; } = [];
}
