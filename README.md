# EPAM DIAL .NET SDK (Preview 🚧)

[![Build](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml)

## What is DIAL?

AI **[DIAL](https://epam-rail.com/platform)** stands for **D**eterministic **I**ntegrator of **A**pplications and **L**anguage Models. It is an enterprise-grade, open-source AI orchestration platform that simplifies the development, deployment, and management of AI-driven applications. AI DIAL acts as both a development studio and an application server, enabling seamless integration between various AI models, data pipelines, and business applications.

See: [DIAL 2.0: The Open Source AI Orchestration Platform Overview - YouTube](https://www.youtube.com/watch?v=Ud2UyXjNK4I&list=PLhkKkML8gp_fNs5NQdztKwIr2yWnLoQfy&index=1)

This repository contains .NET SDK to simplify integration with DIAL platform.


> NB: The EPAM.DIAL.* packages  are in preview and are subject to change. The preview version will be release somewhere after Aspire 9.2 release.

| Package                    | Version                                                                                                                      | Description               |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ------------------------- |
| `Nall.EPAM.DIAL.Aspire.Hosting` | [![Nuget](https://img.shields.io/nuget/v/Nall.EPAM.DIAL.Aspire.Hosting.svg)](https://nuget.org/packages/Nall.EPAM.DIAL.Aspire.Hosting) | Aspire Integration        |
| `Nall.EPAM.DIAL.Aspire`         | [![Nuget](https://img.shields.io/nuget/v/Nall.EPAM.DIAL.Aspire.svg)](https://nuget.org/packages/Nall.EPAM.DIAL.Aspire)                 | Aspire Client Integration |
<!-- | `EPAM.DIAL.Core.Sdk`       | [![Nuget](https://img.shields.io/nuget/v/Nall.EPAM.DIAL.Aspire.Hosting.svg)](https://nuget.org/packages/EPAM.DIAL.Core.Sdk)       | Core API Sdk              | -->

---

> [!NOTE]
> I do not represent EPAM DIAL. This is a personal project to simplify integration with DIAL platform. 

## Features

### Host Integration with Aspire

* Integration with .NET Aspire. Aspire is a great way to simplify the development process.

Install hosting integration for your project:

```bash
dotnet add package Nall.EPAM.DIAL.Aspire.Hosting
```

The code below shows how to create a simple DIAL installation configured to work with two locally installed models: `DeepSeek-R1` and `Phi-3.5`. 

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder
    .AddOllama("ollama")
    .WithOpenWebUI()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
ollama.AddModel("ollama-deepseek-r1", "deepseek-r1:1.5b");
ollama.AddModel("ollama-phi3", "phi3.5");

var dial = builder.AddDial("dial", port: 8080).WaitFor(ollama).WithChatUI(port: 3000);

var deepseek = dial.AddModel("deepseek", deploymentName: "deepseek-r1:1.5b")
    .WithEndpoint(ollama.Resource.PrimaryEndpoint)
    .WithDisplayName("DeepSeek-R1");

var phi3 = dial.AddModel("phi3", deploymentName: "phi3.5")
    .WithEndpoint(ollama.Resource.PrimaryEndpoint)
    .WithDisplayName("Phi-3.5"); 

builder.AddProject<Projects.Api>("api").WithReference(deepseek).WithReference(phi3).WaitFor(dial);

builder.Build().Run();
```

Here is the output from Aspire Dashboard. It shows *Aspire Resources Component Graph*:

![Aspire Graph Demo](./assets/demo/aspire-graph-demo.png)

💡 You are not limited to only self-hosted models. AI DIAL allows you to access models from all major LLM providers, language models from the open-source community, alternative vendors, and fine-tuned micro models, as well as self-hosted or models listed on HuggingFace or DeepSeek.
See [Supported Models](https://docs.dialx.ai/platform/supported-models) for more information.

For example, you can use `OpenAI` models:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var openAIApiKey = builder.AddParameter("azure-openai-api-key", secret: true);
var openai = builder
    .AddAzureOpenAI("openai")
    .ConfigureInfrastructure(infra =>
    {
        var resources = infra.GetProvisionableResources();
        var account = resources.OfType<CognitiveServicesAccount>().Single();
        account.Properties.DisableLocalAuth = false; // so we can use api key
    });
var gpt4 = openai.AddDeployment("az-gpt-4o-mini", "gpt-4o-mini", "2024-07-18");

var dial = builder.AddDial("dial", port: 8080).WithChatUI(port: 3000).WaitFor(gpt4);

var dialOpenAI = dial.AddOpenAIModelAdapter("gpt-4o-mini", deploymentName: "gpt-4o-mini")
    .WithUpstream(gpt4, openAIApiKey)
    .WithDisplayName("gpt-4o-mini")
    .WithDescription("Azure OpenAI")
    .WithWellKnownIcon(WellKnownIcon.GPT4);

builder.AddProject<Projects.Api>("api").WithReference(dialOpenAI).WithReference(gpt4).WaitFor(dial);

builder.Build().Run();
```

The code above shows how to create a simple DIAL installation configured to work with `Azure OpenAI` models.

### DIAL Chat

AI DIAL Chat is a powerful enterprise-grade application that serves as a default web interface for AI DIAL users, providing access to the full set of AI DIAL features.

The great thing about DIAL Chat is that it allows you to run multiple models in parallel. This means you can compare the performance of different models and choose the best one for your use case. 

For example, you can run `DeepSeek-R1` and `Phi-3.5` models in parallel and compare their output:

![Aspire Chat Demo](./assets/demo/compare-models-demo.png)

See [Chat User Guide](https://docs.epam-rail.com/user-guide) for more information.


### DIAL Marketplace

DIAL Marketplace is a comprehensive hub for all applications, language models, and GenAI assistants available in the DIAL environment of your organization.

![Aspire Chat Demo](./assets/demo/marketplace-demo.png)

**Collaboration Center:**
DIAL Marketplace is a powerful platform for fostering collaboration within organizations. It encourages the creation and publishing of custom applications, models, and assistants, thereby enhancing knowledge sharing and fostering GenAI experimentation. As a GenAI collaboration hub, DIAL Marketplace empowers your entire organization, while maintaining all required permissions and roles.

**Development Studio:**
Another powerful feature of DIAL Marketplace is its functionality as a development studio, facilitating the rapid creation of low-code quick apps and providing access to a full-scale Integrated Development Environment (IDE) for code app development and deployment.

See [DIAL Marketplace](https://docs.epam-rail.com/marketplace) for more information.

### Client Integration with Aspire

You can use DIAL Core API to programmatically interact with DIAL. This is useful for building custom applications that need to communicate with DIAL.

For example, you can consume DIAL Completion API to get completions from a model: 

```bash
dotnet add package Nall.EPAM.DIAL.Aspire
```

And modify `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDialOpenAIClient("deepseek").AddChatClient();

var app = builder.Build();

app.MapGet(
    "/chat",
    async ([FromQuery] string query, [FromServices] IChatClient client) =>
    {
        var prompt = $"You are helpful assistant. Answer the following question: '{query}'";
        var response = await client.GetResponseAsync(prompt);

        return Results.Ok(response);
    }
);
app.MapDefaultEndpoints();

app.Run();
```

## Blogs

- [Introducing AI DIAL: The Open-Source AI Orchestration Platform](https://nikiforovall.github.io/dotnet/ai/2025/03/30/introduction-to-dial.html)

## References

* <https://epam-rail.com>
* <https://docs.epam-rail.com>
* <https://github.com/epam/ai-dial>
* <https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions>
* <https://learn.microsoft.com/en-us/dotnet/core/extensions/artificial-intelligence>
* <https://github.com/openai/openai-dotnet/blob/main/docs/observability.md>
