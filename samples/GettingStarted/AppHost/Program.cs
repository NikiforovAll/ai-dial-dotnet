var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder
    .AddOllama("ollama")
    .WithImageTag("0.6.0")
    .WithOpenWebUI(ui => ui.WithImageTag("0.5.20"))
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
ollama.AddModel("ollama-deepseek-r1", "deepseek-r1:1.5b");
ollama.AddModel("ollama-phi3", "phi3.5");

var dial = builder.AddDial("dial", port: 8080).WithChatUI(port: 3000).WaitFor(ollama);

var deepseek = dial.AddModel("deepseek", deploymentName: "deepseek-r1:1.5b")
    .WithEndpoint(ollama.Resource.PrimaryEndpoint)
    .WithDisplayName("DeepSeek-R1")
    .WithDescription(
        "DeepSeek-R1 is a large language model (LLM) with 1.5 billion parameters, designed for high performance and efficiency."
    )
    .WithIconUrl(
        "https://raw.githubusercontent.com/deepseek-ai/DeepSeek-V2/refs/heads/main/figures/logo.svg"
    );

var phi3 = dial.AddModel("phi3", deploymentName: "phi3.5")
    .WithEndpoint(ollama.Resource.PrimaryEndpoint)
    .WithDisplayName("Phi-3.5")
    .WithDescription(
        "A lightweight AI model with 3.8 billion parameters with performance overtaking similarly and larger sized models."
    )
    .WithIconUrl("https://avatars.githubusercontent.com/u/6154722?s=48&v=4");

builder.AddProject<Projects.Api>("api").WithReference(deepseek).WithReference(phi3).WaitFor(dial);

builder.Build().Run();
