using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddDialOpenAIClient("gpt-4o-mini").AddChatClient();

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
