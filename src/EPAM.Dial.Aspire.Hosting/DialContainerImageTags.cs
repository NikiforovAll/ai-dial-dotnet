namespace EPAM.Dial.Aspire.Hosting;

internal static class DialContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "epam/ai-dial-core";
    public const string Tag = "0.25.1";

    public const string ChatRegistry = "docker.io";
    public const string ChatImage = "epam/ai-dial-chat";
    public const string ChatTag = "0.26.0";

    public const string ChatThemesRegistry = "docker.io";
    public const string ChatThemesImage = "epam/ai-dial-chat-themes";
    public const string ChatThemesTag = "0.9.1";
}
