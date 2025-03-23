namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents an Dial server
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DialResource"/> class.
/// </remarks>
/// <param name="name">The name of the resource.</param>
public class DialResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    public EndpointReference PrimaryEndpoint => this.primaryEndpoint ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the Dial http endpoint.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.PrimaryEndpoint.Property(EndpointProperty.Url)}");
}
