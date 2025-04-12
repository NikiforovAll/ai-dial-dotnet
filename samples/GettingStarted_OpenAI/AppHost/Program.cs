using Azure.Provisioning.CognitiveServices;

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
    .WithDescription("Azure OpenAI Service")
    .WithWellKnownIcon(WellKnownIcon.GPT4);

builder.AddProject<Projects.Api>("api").WithReference(dialOpenAI).WithReference(gpt4).WaitFor(dial);

builder.Build().Run();
