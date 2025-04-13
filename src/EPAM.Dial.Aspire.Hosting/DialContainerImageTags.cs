namespace EPAM.Dial.Aspire.Hosting;

internal static class DialCoreContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "epam/ai-dial-core";
    public const string Tag = "0.27.0";

    public const string ChatRegistry = "docker.io";
    public const string ChatImage = "epam/ai-dial-chat";
    public const string ChatTag = "0.27.0";

    public const string ChatThemesRegistry = "docker.io";
    public const string ChatThemesImage = "epam/ai-dial-chat-themes";
    public const string ChatThemesTag = "0.9.1";

    public const string AssistantRegistry = "docker.io";
    public const string AssistantImage = "epam/ai-dial-assistant";
    public const string AssistantTag = "0.7.0";
}


public static class DialImageTags
{
    public const string OpenAIAdapterRegistry = "docker.io";
    public const string OpenAIAdapterImage = "epam/ai-dial-adapter-openai";
    public const string OpenAIAdapterTag = "0.24.0";
}
