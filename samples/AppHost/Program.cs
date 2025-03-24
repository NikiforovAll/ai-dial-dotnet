using EPAM.Dial.Aspire.Hosting.Models;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithOpenWebUI().WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var deepseek = ollama.AddModel("deepseek-r1", "deepseek-r1:1.5b");
var phi3 = ollama.AddModel("phi3", "phi3.5");

builder
    .AddDial("dial", port: 8080)
    .WaitFor(ollama)
    .WithChatUI(port: 3000)
    .AddModel("deepseek-r1:1.5b", DeepSeekR1(ollama))
    .AddModel("phi3.5", Phi3(ollama));

builder.Build().Run();

static DialModel DeepSeekR1(IResourceBuilder<OllamaResource> ollama) =>
    new()
    {
        DisplayName = "DeepSeek-R1",
        Description = "",
        EndpointExpression = ollama.Resource.PrimaryEndpoint.ToDialCompletionEndpoint,
        IconUrl = new Uri("https://raw.githubusercontent.com/deepseek-ai/DeepSeek-V2/refs/heads/main/figures/logo.svg"),
    };

static DialModel Phi3(IResourceBuilder<OllamaResource> ollama) =>
    new()
    {
        DisplayName = "Phi-3.5",
        Description =
            "A lightweight AI model with 3.8 billion parameters with performance overtaking similarly and larger sized models.",
        EndpointExpression = ollama.Resource.PrimaryEndpoint.ToDialCompletionEndpoint,
        IconUrl = new Uri("https://avatars.githubusercontent.com/u/6154722?s=48&v=4"),
    };
