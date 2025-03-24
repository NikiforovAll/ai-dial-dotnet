namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting;
using EPAM.Dial.Aspire.Hosting.Models;

/// <summary>
/// Provides extension methods for adding Dial to the application model.
/// </summary>
public static class DialBuilderExtensions
{
    /// <summary>
    /// Adds an DIAL Core
    /// </summary>
    public static IResourceBuilder<DialResource> AddDial(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int? port = null
    )
    {
        var dial = new DialResource(name);

        var cache = builder
            .AddRedis($"{dial.Name}-cache")
            .WithImage("redis:7.2.4-alpine3.19")
            .WithParentRelationship(dial);

        var resource = builder
            .AddResource(dial)
            .WithImage(DialContainerImageTags.Image, DialContainerImageTags.Tag)
            .WithImageRegistry(DialContainerImageTags.Registry)
            .WithHttpEndpoint(port: port, targetPort: 8080, DialResource.PrimaryEndpointName)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add("AIDIAL_SETTINGS", "/opt/settings/settings.json");
                context.EnvironmentVariables.Add("JAVA_OPTS", "-Dgflog.config=/opt/settings/gflog.xml");
                context.EnvironmentVariables.Add("LOG_DIR", "/app/log");
                context.EnvironmentVariables.Add("STORAGE_DIR", "/app/data");
                context.EnvironmentVariables.Add("aidial.config.files", "[\"/opt/settings/config.json\"]");
                context.EnvironmentVariables.Add(
                    "aidial.storage.overrides",
                    /*lang=json,strict*/
                    "{ \"jclouds.filesystem.basedir\": \"data\" }"
                );
                context.EnvironmentVariables.Add(
                    "aidial.redis.singleServerConfig.address",
                    $"redis://{cache.Resource.Name}:{cache.Resource.PrimaryEndpoint.TargetPort}"
                );
            })
            .WithContainerFiles(destinationPath: "/opt", callback: (_, _) => DialConfigFiles(dial))
            .WaitFor(cache)
            .PublishAsContainer();

        return resource;
    }

    public static IResourceBuilder<DialModelResource> AddModel(
        this IResourceBuilder<DialResource> builder,
        string name,
        DialModel model
    )
    {
        model.ModelName ??= name;
        var modelResource = new DialModelResource(name, model.ModelName, builder.Resource);

        var modelBuilder = builder
            .ApplicationBuilder.AddResource(modelResource)
            .WithParentRelationship(builder.Resource);

        builder.Resource.AddModel(model);

        return modelBuilder;
    }

    private static Task<IEnumerable<ContainerFileSystemItem>> DialConfigFiles(DialResource dial) =>
        Task.FromResult<IEnumerable<ContainerFileSystemItem>>(
            [
                new ContainerDirectory
                {
                    Name = "settings",
                    Entries =
                    [
                        new ContainerFile
                        {
                            Name = "settings.json",
                            Contents = /*lang=json,strict*/
                            """
                            {
                                "server": {
                                    "port": 8080
                                },
                                "identityProviders": {
                                    "test": {
                                    "rolePath": "roles",
                                    "disableJwtVerification": true
                                    }
                                },
                                "encryption": {
                                    "secret": "salt",
                                    "key": "password"
                                }
                            }
                            """,
                        },
                        new ContainerFile
                        {
                            Name = "gflog.xml",
                            Contents = """
                            <config>
                                <appender name="console" factory="com.epam.deltix.gflog.core.appender.ConsoleAppenderFactory"/>
                                <appender name="file" factory="com.epam.deltix.gflog.core.appender.DailyRollingFileAppenderFactory" bufferCapacity="32m" file="/app/log/aidial.log" maxFiles="10" maxFileSize="1g">
                                    <layout template="%m%n"/>
                                </appender>

                                <logger level="INFO">
                                    <appender-ref ref="console"/>
                                </logger>

                                <logger level="INFO" name="aidial.log">
                                    <appender-ref ref="file"/>
                                </logger>

                                <service entryEncoding="UTF-8" entryMaxCapacity="30m" bufferCapacity="128m"/>
                            </config>
                            """,
                        },
                        new ContainerFile
                        {
                            Name = "config.json",
                            Contents = /*lang=json,strict*/
                            $$"""
                            {
                                "routes": {},
                                "applications": {},
                                "models": {{DialModel.ToJson(dial.Models)}},
                                "keys": {
                                    "dial_api_key": {
                                        "project": "TEST-PROJECT",
                                        "role": "default"
                                    }
                                },
                                "roles": {
                                    "default": {
                                        "limits": {{DialModel.ToLimitsJson(dial.Models)}}
                                    }
                                }
                            }
                            """,
                        },
                    ],
                },
            ]
        );

    /// <summary>
    /// Adds a data volume to the DIAL container.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<DialResource> WithDataVolume(
        this IResourceBuilder<DialResource> builder,
        string? name = null,
        bool isReadOnly = false
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "dial"), "/app/data", isReadOnly);
    }

    /// <summary>
    /// Adds a Chat DIAL
    /// </summary>
    public static IResourceBuilder<T> WithChatUI<T>(
        this IResourceBuilder<T> builder,
        [ResourceName] string? name = null,
        int? port = null,
        Action<IResourceBuilder<DialChatResource>>? configureContainer = null
    )
        where T : DialResource
    {
        name ??= $"{builder.Resource.Name}-chat";

        var chatThemes = new DialChatThemesResource($"{name}-themes");
        var themesBuilder = builder
            .ApplicationBuilder.AddResource(chatThemes)
            .WithImage(DialContainerImageTags.ChatThemesImage, DialContainerImageTags.ChatThemesTag)
            .WithImageRegistry(DialContainerImageTags.ChatThemesRegistry)
            .WithHttpEndpoint(targetPort: 8080, name: "http");

        var chat = new DialChatResource(name);
        var chatBuilder = builder
            .ApplicationBuilder.AddResource(chat)
            .WithImage(DialContainerImageTags.ChatImage, DialContainerImageTags.ChatTag)
            .WithImageRegistry(DialContainerImageTags.ChatRegistry)
            .WithHttpEndpoint(port: port, targetPort: 3000, name: "http")
            .WithEnvironment(context => ConfigureDialChat(context, builder.Resource, themesBuilder.Resource))
            .WaitFor(themesBuilder)
            .WaitFor(builder);

        configureContainer?.Invoke(chatBuilder);

        chatBuilder.WithParentRelationship(builder.Resource);
        themesBuilder.WithParentRelationship(builder.Resource);

        return builder;
    }

    private static void ConfigureDialChat(
        EnvironmentCallbackContext context,
        DialResource resource,
        DialChatThemesResource themesResource
    )
    {
        context.EnvironmentVariables.Add(
            "ENABLED_FEATURES",
            "conversations-section,prompts-section,top-settings,top-clear-conversation,top-chat-info,top-chat-model-settings,empty-chat-settings,header,footer,request-api-key,report-an-issue,likes,conversations-sharing,input-files,attachments-manager,prompts-sharing,prompts-publishing,conversations-publishing,custom-logo,input-links,custom-applications,message-templates,marketplace,quick-apps,code-apps,applications-sharing,marketplace-table-view"
        );
        context.EnvironmentVariables.Add("KEEP_ALIVE_TIMEOUT", "20000");
        context.EnvironmentVariables.Add("NEXTAUTH_SECRET", "secret");
        context.EnvironmentVariables.Add(
            "DIAL_API_HOST",
            $"{resource.PrimaryEndpoint.Scheme}://{resource.Name}:{resource.PrimaryEndpoint.TargetPort}"
        );
        context.EnvironmentVariables.Add("DIAL_API_KEY", "dial_api_key");
        context.EnvironmentVariables.Add(
            "THEMES_CONFIG_HOST",
            $"{themesResource.PrimaryEndpoint.Scheme}://{themesResource.Name}:{themesResource.PrimaryEndpoint.TargetPort}"
        );
    }
}
