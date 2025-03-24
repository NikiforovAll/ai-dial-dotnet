namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

public static class DialUtilities
{
    /// <summary>
    /// Converts the endpoint to a Dial-compatible endpoint.
    /// </summary>
    /// <remarks>
    /// DIAL sends OpenAI compatible requests from DIAL Chat, we utilize it for Ollama integration because it also
    /// accepts OpenAI compatible requests.
    /// </remarks>
    public static string ToDialCompletionEndpoint(this EndpointReference endpoint) =>
        $"{endpoint.Scheme}://{endpoint.Resource.Name}:{endpoint.TargetPort}/v1/chat/completions";
}
