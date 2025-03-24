namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialModelResource(string name, DialResource dial) : Resource(name), IResourceWithParent<DialResource>
{
    public DialResource Parent { get; } = dial;
}
