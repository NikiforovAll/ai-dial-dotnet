using Azure.Provisioning.CognitiveServices;

var builder = DistributedApplication.CreateBuilder(args);

var openAIApiKey = builder.AddParameter("azure-openai-api-key", secret: true);

// alternatively, you can use a connection string to connect to specify the upstream
// var cs = builder.AddConnectionString(
//     "openai-as-connection-string",
//     ReferenceExpression.Create($"Endpoint=https://api.openai.com/v1;Key={openAIApiKey};")
// );

var openai = builder
    .AddAzureOpenAI("openai")
    .ConfigureInfrastructure(infra =>
    {
        var resources = infra.GetProvisionableResources();
        var account = resources.OfType<CognitiveServicesAccount>().Single();
        account.Properties.DisableLocalAuth = false; // so we can use api key
    });
var gpt4o = openai.AddDeployment("gpt-4o", "gpt-4o", "2024-08-06");

var dial = builder.AddDial("dial", port: 8080).WithChatUI(port: 3000).WaitFor(gpt4o);

var dialGpt4o = dial.AddOpenAIModelAdapter("dial-gpt-4o", deploymentName: "gpt-4o")
    .WithUpstream(gpt4o, openAIApiKey)
    .WithDisplayName("gpt-4o")
    .WithDescription(
        "gpt-4o is engineered for speed and efficiency. Its advanced ability to handle complex queries with minimal resources can translate into cost savings and performance."
    )
    .WithWellKnownIcon(WellKnownIcon.GPT4);

var todoAddonContainer = builder
    .AddDockerfile("todo-addon-container", contextPath: "addons", dockerfilePath: "TodoDockerfile")
    .WithHttpEndpoint(port: null, targetPort: 5003, "http");

var todoAddon = dial.AddAddon("todo-addon")
    .WithUpstream(todoAddonContainer)
    .WithDisplayName("TODO List")
    .WithDescription("Addon that allows to manage user's TODO list.");

var assistantBuilder = dial.AddAssistantsBuilder()
    .WithImage("nikiforovall/ai-dial-assistant", "8.0.1-rc");
var todoAssistant = assistantBuilder
    .AddAssistant("todo-assistant")
    .WithPrompt(
        "You are assistant that helps to manage TODO list for the user. You can add, remove and view your TODOs."
    )
    .WithDescription(
        "The assistant that manages your TODO list. It can add, remove and view your TODOs."
    )
    .WithDisplayName("TODO Assistant")
    .WithAddon(todoAddon);

builder
    .AddProject<Projects.Api>("api")
    .WithReference(todoAssistant)
    .WithReference(dialGpt4o)
    .WithReference(gpt4o)
    .WaitFor(dial);

builder.Build().Run();
