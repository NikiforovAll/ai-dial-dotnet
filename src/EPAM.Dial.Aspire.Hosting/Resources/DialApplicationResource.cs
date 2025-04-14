namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialApplicationResource(string name, DialResource dial)
    : IResourceWithEndpoints,
        IResourceWithParent<DialResource>
{
    public DialResource Parent { get; } = dial;

    public string Name { get; } = name;

    public ResourceAnnotationCollection Annotations => [];

    public EndpointReference? PrimaryEndpoint { get; set; }

    public string? DisplayName { get; set; }

    public string? Description { get; set; }
}
