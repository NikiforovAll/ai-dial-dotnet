# EPAM DIAL .NET SDK

[![Build](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/NikiforovAll/epam-dial-dotnet/actions/workflows/build.yml)

## What is DIAL?

AI **[DIAL](https://docs.epam-rail.com/)** stands for **D**eterministic **I**ntegrator of **A**pplications and **L**anguage Models. It is an enterprise-grade, open-source AI orchestration platform that simplifies the development, deployment, and management of AI-driven applications. AI DIAL acts as both a development studio and an application server, enabling seamless integration between various AI models, data pipelines, and business applications.

![DIAL Platform](./assets/arch.png)

This repository contains .NET SDK to simplify integration with DIAL platform. 

> [!IMPORTANT]  
> I do not represent EPAM DIAL. This is a personal project.

| Package                    | Version                                                                                                                      | Description        |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| `Epam.Dial.Aspire.Hosting` | [![Nuget](https://img.shields.io/nuget/v/Epam.Dial.Aspire.Hosting.svg)](https://nuget.org/packages/Epam.Dial.Aspire.Hosting) | Aspire Integration |
| `Epam.Dial.Core.Sdk`       | [![Nuget](https://img.shields.io/nuget/v/Epam.Dial.Aspire.Hosting.svg)](https://nuget.org/packages/Epam.Dial.Aspire.Hosting) | Core API Sdk       |


## Features

### Integration with Aspire

* Integration with .NET Aspire. Aspire is a great way to simplify the development process.

Install hosting integration for your project:

```bash
dotnet add package EPAM.Dial.Aspire.Hosting
```

Modify `AppHost.cs`:

The code below shows how to create a simple DIAL installation configured to work with two locally installed models: `DeepSeek-R1` and `Phi-3.5`. 

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithOpenWebUI().WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var deepseek = ollama.AddModel("deepseek-r1", "deepseek-r1:1.5b");
var phi3 = ollama.AddModel("phi3", "phi3.5");

builder
    .AddDial("dial", port: 8080)
    .WithChatUI(port: 3000)
    .AddModel("deepseek-r1:1.5b", DeepSeekR1(ollama))
    .AddModel("phi3.5", Phi3(ollama));

builder.Build().Run();

static DialModel DeepSeekR1(IResourceBuilder<OllamaResource> ollama) =>
    new()
    {
        DisplayName = "DeepSeek-R1",
        EndpointExpression = ollama.Resource.PrimaryEndpoint.ToDialCompletionEndpoint,
        IconUrl = new Uri("https://raw.githubusercontent.com/deepseek-ai/DeepSeek-V2/refs/heads/main/figures/logo.svg"),
    };

static DialModel Phi3(IResourceBuilder<OllamaResource> ollama) =>
    new()
    {
        DisplayName = "Phi-3.5",
        EndpointExpression = ollama.Resource.PrimaryEndpoint.ToDialCompletionEndpoint,
        IconUrl = new Uri("https://avatars.githubusercontent.com/u/6154722?s=48&v=4"),
    };
```

Here is output 

![Aspire Graph Demo](./assets/demo/aspire-graph-demo.png)


## Scope

* [X] [Self-Hosted Models](https://docs.epam-rail.com/tutorials/quick-start-with-self-hosted-model)
* [ ] [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions) + Client Integration
* [ ] Demo: API -> DIAL, DIAL Chat -> DIAL
* [ ] [Adapters](https://docs.epam-rail.com/tutorials/quick-start-model)
* [ ] [Applications](https://docs.epam-rail.com/tutorials/quick-start-with-application)
* [ ] Core API Sdk
* [ ] List DIAL configuration resource command
* [ ] Flexible configuration
## Notes

* Dial Chat send requests [compatible with OpenAI API](https://ollama.readthedocs.io/en/openai/?h=openai). 

## References

* <https://epam-rail.com>
* <https://docs.epam-rail.com>
<* https://github.com/epam/ai-dial>
