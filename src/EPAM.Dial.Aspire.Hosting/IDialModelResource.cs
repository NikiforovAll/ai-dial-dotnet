namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public interface IDialModelResource
    : IResourceWithParent<DialResource>,
        IResourceWithConnectionString
{
    public string DeploymentName { get; }

    public string? DisplayName { get; internal set; }

    public string? IconUrl { get; internal set; }

    public WellKnownIcon? WellKnownIcon { get; internal set; }

    public EndpointReference? Endpoint { get; internal set; }

    public string? Description { get; set; }
}
