namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialAssistantBuilderResource(string name, DialResource dial)
    : ContainerResource(name),
        IResourceWithParent<DialResource>,
        IResourceWithConnectionString
{
    public DialResource Parent { get; } = dial;

    internal const string PrimaryEndpointName = "http";

    private EndpointReference? primaryEndpoint;

    public EndpointReference PrimaryEndpoint =>
        this.primaryEndpoint ??= new(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"Endpoint={this.PrimaryEndpoint.Property(EndpointProperty.Scheme)}://{this.PrimaryEndpoint.Property(EndpointProperty.Host)}:{this.PrimaryEndpoint.Property(EndpointProperty.Port)}"
        );

    public IList<DialAssistantResource> Assistants { get; } = [];
}

public class DialAssistantResource(string name, DialAssistantBuilderResource bulider)
    : IResourceWithParent<DialAssistantBuilderResource>,
        IResourceWithConnectionString
{
    public DialAssistantBuilderResource Parent { get; } = bulider;

    public string Name { get; } = name;

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.Parent};Deployment={this.Parent.Name}");

    public ResourceAnnotationCollection Annotations => [];

    public string Prompt { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public IList<DialAddonResource> Addons { get; } = [];
}

public class DialAddonResource(string name, DialResource dial)
    : IResourceWithEndpoints,
        IResourceWithParent<DialResource>
{
    public DialResource Parent { get; } = dial;

    public string Name { get; } = name;

    public EndpointReference? PrimaryEndpoint { get; set; }

    public string? Path { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public ResourceAnnotationCollection Annotations => [];
}
