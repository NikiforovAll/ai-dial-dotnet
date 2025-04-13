using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddKeyedDialOpenAIClient("dial-gpt-4o").AddKeyedChatClient();
builder.AddKeyedDialOpenAIClient("todo-assistant").AddKeyedChatClient();

var app = builder.Build();

app.MapGet(
    "/chat",
    async ([FromQuery] string query, [FromKeyedServices("dial-gpt-4o")] IChatClient client) =>
    {
        var prompt = $"You are helpful assistant. Answer the following question: '{query}'";
        var response = await client.GetResponseAsync(prompt);

        return Results.Ok(response);
    }
);
app.MapGet(
    "/chat-assistant",
    async ([FromQuery] string query, [FromKeyedServices("todo-assistant")] IChatClient client) =>
    {
        var response = await client.GetResponseAsync(query);

        return Results.Ok(response);
    }
);

app.MapDefaultEndpoints();

app.Run();
