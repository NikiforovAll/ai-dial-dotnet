# EPAM DIAL .NET SDK

[![Build](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml)

## What is DIAL?

AI **[DIAL](https://epam-rail.com/platform)** stands for **D**eterministic **I**ntegrator of **A**pplications and **L**anguage Models. It is an enterprise-grade, open-source AI orchestration platform that simplifies the development, deployment, and management of AI-driven applications. AI DIAL acts as both a development studio and an application server, enabling seamless integration between various AI models, data pipelines, and business applications.

This repository contains .NET SDK to simplify integration with DIAL platform.

| Package                    | Version                                                                                                                      | Description        |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| `Epam.Dial.Aspire.Hosting` | [![Nuget](https://img.shields.io/nuget/v/Epam.Dial.Aspire.Hosting.svg)](https://nuget.org/packages/Epam.Dial.Aspire.Hosting) | Aspire Integration |
| `Epam.Dial.Aspire`         | [![Nuget](https://img.shields.io/nuget/v/Epam.Dial.Aspire.svg)](https://nuget.org/packages/Epam.Dial.Aspire)                 | Client Integration |
| `Epam.Dial.Core.Sdk`       | [![Nuget](https://img.shields.io/nuget/v/Epam.Dial.Aspire.Hosting.svg)](https://nuget.org/packages/Epam.Dial.Aspire.Hosting) | Core API Sdk       |

---

> [!NOTE]
> I do not represent EPAM DIAL. This is a personal project to simplify integration with DIAL platform because I find it useful. 

## Features

### Host Integration with Aspire

* Integration with .NET Aspire. Aspire is a great way to simplify the development process.

Install hosting integration for your project:

```bash
dotnet add package EPAM.Dial.Aspire.Hosting
```

Modify `AppHost.cs`:

The code below shows how to create a simple DIAL installation configured to work with two locally installed models: `DeepSeek-R1` and `Phi-3.5`. 

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder
    .AddOllama("ollama")
    .WithOpenWebUI()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

ollama.AddModel("ollama-deepseek-r1", "deepseek-r1:1.5b");
ollama.AddModel("ollama-phi3", "phi3.5");

var dial = builder
    .AddDial("dial", port: 8080)
    .WaitFor(ollama)
    .WithChatUI(port: 3000)
    .WithLifetime(ContainerLifetime.Persistent);

var deepseek = dial.AddModel("deepseek", DeepSeekR1(ollama.Resource.PrimaryEndpoint));
var phi3 = dial.AddModel("phi3", Phi3(ollama.Resource.PrimaryEndpoint));

builder.AddProject<Projects.Api>("api")
    .WithReference(deepseek)
    .WithReference(phi3)
    .WaitFor(dial);

builder.Build().Run();
```

Here is the output from Aspire Dashboard. It shows *Aspire Resources Component Graph*:

![Aspire Graph Demo](./assets/demo/aspire-graph-demo.png)

### DIAL Chat


The great thing about DIAL Chat is that it allows you to run multiple models in parallel. This means you can compare the performance of different models and choose the best one for your use case. 

For example, you can run `DeepSeek-R1` and `Phi-3.5` models in parallel and compare their output:


![Aspire Chat Demo](./assets/demo/compare-models-demo.png)

### Client Integration with Aspire

You can use DIAL Core API to programmatically interact with DIAL. This is useful for building custom applications that need to communicate with DIAL.

For example, you can consume DIAL Completion API to get completions from a model: 

```bash
dotnet add package EPAM.Dial.Aspire
```

And modify `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDialApiClient("deepseek").AddChatClient();

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

## Scope

* [X] [Self-Hosted Models](https://docs.epam-rail.com/tutorials/quick-start-with-self-hosted-model)
* [X] [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions) + Client Integration
* [X] Demo: API -> DIAL, DIAL Chat -> DIAL
* [ ] Configure [Applications](https://docs.epam-rail.com/tutorials/quick-start-with-application)
* [ ] Configure [Adapters](https://docs.epam-rail.com/tutorials/quick-start-model)
* [ ] MVP release should be based on [Aspire 9.2.0](https://www.nuget.org/packages/Aspire.Hosting)
* [ ] Python Application + Aspire
* [ ] .NET Application + Aspire
* [ ] Core API Sdk
* [ ] List DIAL configuration resource command
* [ ] Make configuration flexible and extensible

## References

* <https://epam-rail.com>
* <https://docs.epam-rail.com>
* <https://github.com/epam/ai-dial>
* <https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions>
* <https://learn.microsoft.com/en-us/dotnet/core/extensions/artificial-intelligence>
* <https://github.com/openai/openai-dotnet/blob/main/docs/observability.md>
