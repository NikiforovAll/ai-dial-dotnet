using Azure.Provisioning.CognitiveServices;
using EPAM.Dial.Aspire.Hosting.Models;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithOpenWebUI().WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
ollama.AddModel("ollama-deepseek-r1", "deepseek-r1:1.5b");
ollama.AddModel("ollama-phi3", "phi3.5");

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
    new AzureOpenAIDeployment(name: "gpt-4o-mini", modelName: "gpt-4o-mini", modelVersion: "2024-07-18")
);

var dial = builder.AddDial("dial", port: 8080).WaitFor(ollama).WaitFor(gpt4).WithChatUI(port: 3000);

var dialDeepseek = dial.AddModel("deepseek", DeepSeekR1(ollama.Resource.PrimaryEndpoint));
var dialPhi3 = dial.AddModel("phi3", Phi3(ollama.Resource.PrimaryEndpoint));
var dialOpenAI = dial.AddOpenAIModelAdapter("gpt-4o-mini", AzureOpenAI(gpt4.Resource, openAIApiKey.Resource));

builder
    .AddProject<Projects.Api>("api")
    .WithReference(dialDeepseek)
    .WithReference(dialPhi3)
    .WithReference(dialOpenAI)
    .WaitFor(dial);

builder.Build().Run();

static DialModel DeepSeekR1(EndpointReference endpoint) =>
    new()
    {
        DisplayName = "DeepSeek-R1",
        DeploymentName = "deepseek-r1:1.5b",
        Description = "",
        EndpointExpression = () => AsCompletionEndpoint(endpoint),
        IconUrl = new Uri("https://raw.githubusercontent.com/deepseek-ai/DeepSeek-V2/refs/heads/main/figures/logo.svg"),
    };

static DialModel Phi3(EndpointReference endpoint) =>
    new()
    {
        DisplayName = "Phi-3.5",
        DeploymentName = "phi3.5",
        Description =
            "A lightweight AI model with 3.8 billion parameters with performance overtaking similarly and larger sized models.",
        EndpointExpression = () => AsCompletionEndpoint(endpoint),
        IconUrl = new Uri("https://avatars.githubusercontent.com/u/6154722?s=48&v=4"),
    };

static DialModel AzureOpenAI(AzureOpenAIResource openAi, ParameterResource apiKey) =>
    new()
    {
        DisplayName = "gpt-4o-mini",
        Description = "Azure OpenAI Service",
        Upstreams = [() => new ModelUpstream { Endpoint = AsOpenAIEndpoint(openAi), Key = apiKey.Value }],
        IconUrl = new Uri(
            "https://raw.githubusercontent.com/epam/ai-dial-chat-themes/refs/heads/development/static/gpt4.svg"
        ),
    };

static string AsCompletionEndpoint(EndpointReference endpoint) =>
    $"{endpoint.Scheme}://{endpoint.Resource.Name}:{endpoint.TargetPort}/v1/chat/completions";

static string AsOpenAIEndpoint(AzureOpenAIResource openAi)
{
    var connectionString =
        openAi.ConnectionString.Value ?? throw new InvalidOperationException("Connection string is not set.");
    var endpoint =
        connectionString
            .Split(';')
            .FirstOrDefault(x => x.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=')[1] ?? string.Empty;

    var deploymentName = openAi.Deployments[0].Name;

    return $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions";
}
