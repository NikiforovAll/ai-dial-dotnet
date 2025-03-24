namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting.Models;

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
        ReferenceExpression.Create(
            $"Endpoint={this.PrimaryEndpoint.Property(EndpointProperty.Scheme)}://{this.PrimaryEndpoint.Property(EndpointProperty.Host)}:{this.PrimaryEndpoint.Property(EndpointProperty.Port)};Key=dial_api_key"
        );

    private readonly Dictionary<string, DialModel> models = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// A dictionary where the key is the resource name and the value is the database name.
    /// </summary>
    public IReadOnlyDictionary<string, DialModel> Models => this.models;

    internal void AddModel(DialModel model)
    {
        this.models.TryAdd(model.DeploymentName, model);
    }
}
