var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithDataVolume();

var model = ollama.AddHuggingFaceModel("deepseek", "DeepSeek-R1-Distill-Qwen-1.5B");

builder.AddDial("dial", port: 8080).WithChatUI(port: 3000);

builder.Build().Run();
