namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents an DIAL Chat resource
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DialChatResource"/> class.
/// </remarks>
/// <param name="name">The name of the resource.</param>
public class DialChatResource(string name) : ContainerResource(name), IResourceWithEndpoints
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    /// <summary>
    /// Gets the http endpoint for the DIAL Chat resource.
    /// </summary>
    public EndpointReference PrimaryEndpoint => this.primaryEndpoint ??= new(this, PrimaryEndpointName);
}
