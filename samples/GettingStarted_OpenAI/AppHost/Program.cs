using Azure.Provisioning.CognitiveServices;
using EPAM.Dial.Aspire.Hosting.Models;

var builder = DistributedApplication.CreateBuilder(args);

var openAIApiKey = builder.AddParameter("azure-openai-api-key", secret: true);
var openai = builder
    .AddAzureOpenAI("openai")
    .ConfigureInfrastructure(infra =>
    {
        var resources = infra.GetProvisionableResources();
        var account = resources.OfType<CognitiveServicesAccount>().Single();
        account.Properties.DisableLocalAuth = false;
    });
var gpt4 = openai.AddDeployment(
    new AzureOpenAIDeployment(
        name: "gpt-4o-mini",
        modelName: "gpt-4o-mini",
        modelVersion: "2024-07-18"
    )
);

var dial = builder.AddDial("dial", port: 8080).WaitFor(gpt4).WithChatUI(port: 3000);

var dialOpenAI = dial.AddOpenAIModelAdapter(
    "gpt-4o-mini",
    AzureOpenAI(gpt4.Resource, openAIApiKey.Resource)
);

builder.AddProject<Projects.Api>("api").WithReference(dialOpenAI).WaitFor(dial);

builder.Build().Run();

static DialModel AzureOpenAI(AzureOpenAIResource openAi, ParameterResource apiKey) =>
    new()
    {
        DisplayName = "gpt-4o-mini",
        Description = "Azure OpenAI Service",
        Upstreams =
        [
            () => new ModelUpstream { Endpoint = AsOpenAIEndpoint(openAi), Key = apiKey.Value },
        ],
        IconUrl = new Uri(
            "https://raw.githubusercontent.com/epam/ai-dial-chat-themes/refs/heads/development/static/gpt4.svg"
        ),
    };

static string AsOpenAIEndpoint(AzureOpenAIResource openAi)
{
    var connectionString =
        openAi.ConnectionString.Value
        ?? throw new InvalidOperationException("Connection string is not set.");
    var endpoint =
        connectionString
            .Split(';')
            .FirstOrDefault(x => x.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=')[1] ?? string.Empty;

    var deploymentName = openAi.Deployments[0].Name;

    return $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions";
}
