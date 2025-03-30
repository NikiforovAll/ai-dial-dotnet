using EPAM.Dial.Aspire.Hosting.Models;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder
    .AddOllama("ollama")
    .WithOpenWebUI()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
ollama.AddModel("ollama-deepseek-r1", "deepseek-r1:1.5b");
ollama.AddModel("ollama-phi3", "phi3.5");

var dial = builder.AddDial("dial", port: 8080).WaitFor(ollama).WithChatUI(port: 3000);

var dialDeepseek = dial.AddModel("deepseek", DeepSeekR1(ollama.Resource.PrimaryEndpoint));
var dialPhi3 = dial.AddModel("phi3", Phi3(ollama.Resource.PrimaryEndpoint));

builder
    .AddProject<Projects.Api>("api")
    .WithReference(dialDeepseek)
    .WithReference(dialPhi3)
    .WaitFor(dial);

builder.Build().Run();

static DialModel DeepSeekR1(EndpointReference endpoint) =>
    new()
    {
        DisplayName = "DeepSeek-R1",
        DeploymentName = "deepseek-r1:1.5b",
        Description = "",
        EndpointExpression = () => AsOllamaCompletionEndpoint(endpoint),
        IconUrl = new Uri(
            "https://raw.githubusercontent.com/deepseek-ai/DeepSeek-V2/refs/heads/main/figures/logo.svg"
        ),
    };

static DialModel Phi3(EndpointReference endpoint) =>
    new()
    {
        DisplayName = "Phi-3.5",
        DeploymentName = "phi3.5",
        Description =
            "A lightweight AI model with 3.8 billion parameters with performance overtaking similarly and larger sized models.",
        EndpointExpression = () => AsOllamaCompletionEndpoint(endpoint),
        IconUrl = new Uri("https://avatars.githubusercontent.com/u/6154722?s=48&v=4"),
    };

static string AsOllamaCompletionEndpoint(EndpointReference endpoint) =>
    $"{endpoint.Scheme}://{endpoint.Resource.Name}:{endpoint.TargetPort}/v1/chat/completions";
