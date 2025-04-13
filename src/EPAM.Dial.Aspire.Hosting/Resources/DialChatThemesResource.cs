namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialChatThemesResource(string name) : ContainerResource(name), IResourceWithEndpoints
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    /// <summary>
    /// Gets the http endpoint for the DIAL Chat resource.
    /// </summary>
    public EndpointReference PrimaryEndpoint => this.primaryEndpoint ??= new(this, PrimaryEndpointName);
}
