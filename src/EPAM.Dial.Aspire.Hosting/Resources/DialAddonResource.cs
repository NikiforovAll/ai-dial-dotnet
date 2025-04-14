namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

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
