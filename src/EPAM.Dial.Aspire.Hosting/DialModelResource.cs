namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public class DialModelResource(string name, string modelName, DialResource dial)
    : Resource(name),
        IResourceWithParent<DialResource>,
        IResourceWithConnectionString
{
    public DialResource Parent { get; } = dial;

    /// <summary>
    /// Gets the model name.
    /// </summary>
    public string ModelName { get; } = modelName;

    /// <summary>
    /// Gets the connection string expression for the Ollama model.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{this.Parent};Model={this.ModelName}");
}
