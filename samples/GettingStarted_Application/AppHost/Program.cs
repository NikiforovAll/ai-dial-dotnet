var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api");
var api2 = builder
    .AddDockerfile("api2", "../../..", "samples/GettingStarted_Application/Api/Dockerfile")
    .WithHttpEndpoint(port: null, targetPort: 5181, "http");

var dial = builder.AddDial("dial").WithChatUI(port: 3000).WaitFor(api);
dial.AddApplication("echo")
    // ISSUE with PODMAN: https://github.com/dotnet/aspire/issues/6547
    // This scenario doesn't work because the DIAL Server can't access host network
    // .WithUpstream(api)
    .WithUpstream(api2)
    .WithDisplayName("My Echo App")
    .WithDescription("Simple application that repeats user's message");

builder.Build().Run();
