namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using EPAM.Dial.Aspire.Hosting;
using EPAM.Dial.Aspire.Hosting.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        // TODO: how to replace cache?
        var cache = builder
            .AddRedis($"{dial.Name}-cache")
            .WithImage("redis:7.2.4-alpine3.19")
            .WithParentRelationship(dial);

        var resource = builder
            .AddResource(dial)
            .WithImage(DialCoreContainerImageTags.Image, DialCoreContainerImageTags.Tag)
            .WithImageRegistry(DialCoreContainerImageTags.Registry)
            .WithHttpEndpoint(port: port, targetPort: 8080, DialResource.PrimaryEndpointName)
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables.Add("AIDIAL_SETTINGS", "/opt/settings/settings.json");
                context.EnvironmentVariables.Add(
                    "JAVA_OPTS",
                    "-Dgflog.config=/opt/settings/gflog.xml"
                );
                context.EnvironmentVariables.Add("LOG_DIR", "/app/log");
                context.EnvironmentVariables.Add("STORAGE_DIR", "/app/data");
                context.EnvironmentVariables.Add(
                    "aidial.config.files",
                    "[\"/opt/settings/config.json\"]"
                );
                context.EnvironmentVariables.Add(
                    "aidial.storage.overrides",
                    /*lang=json,strict*/
                    "{ \"jclouds.filesystem.basedir\": \"data\" }"
                );
                var redisConnectionString =
                    $"redis://:{cache.Resource.PasswordParameter!.Value}@{cache.Resource.Name}:{cache.Resource.PrimaryEndpoint.TargetPort}";
                context.EnvironmentVariables.Add(
                    "aidial.redis.singleServerConfig.address",
                    redisConnectionString
                );
            })
            .WithReference(cache)
            .WithContainerFiles(
                destinationPath: "/opt",
                callback: (ctx, _) => DialConfigFiles(ctx, dial)
            )
            .WaitFor(cache)
            .PublishAsContainer();

        return resource;
    }

    private static Task<IEnumerable<ContainerFileSystemItem>> DialConfigFiles(
        ContainerFileSystemCallbackContext ctx,
        DialResource dial
    )
    {
        var logger = ctx
            .ServiceProvider.GetRequiredService<ResourceLoggerService>()
            .GetLogger(dial);

        var limits = DialModelDescriptorExtensions.ToLimitsJson(dial.Models);
        var configJson = new ContainerFile
        {
            Name = "config.json",
            Contents = /*lang=json,strict*/
                $$"""
                {
                    "routes": {},
                    "applications": {},
                    "models": {{dial.Models.Values.ToJson()}},
                    "keys": {
                        "dial_api_key": {
                            "project": "TEST-PROJECT",
                            "role": "default"
                        }
                    },
                    "roles": {
                        "default": {
                            "limits": {{limits}}
                        }
                    }
                }
                """,
        };
        logger.LogInformation("Adding config.json:");
        logger.LogInformation("{ConfigJson}", configJson.Contents);

        return Task.FromResult<IEnumerable<ContainerFileSystemItem>>(
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
                        configJson,
                    ],
                },
            ]
        );
    }

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

        return builder.WithVolume(
            name ?? VolumeNameGenerator.Generate(builder, "dial"),
            "/app/data",
            isReadOnly
        );
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
            .WithImage(
                DialCoreContainerImageTags.ChatThemesImage,
                DialCoreContainerImageTags.ChatThemesTag
            )
            .WithImageRegistry(DialCoreContainerImageTags.ChatThemesRegistry)
            .WithHttpEndpoint(targetPort: 8080, name: "http")
            .WithUrlForEndpoint(
                "http",
                r =>
                {
                    r.DisplayText = "Chat Themes Config";
                    r.Url += "/config.json";
                }
            );

        var chat = new DialChatResource(name);
        var chatBuilder = builder
            .ApplicationBuilder.AddResource(chat)
            .WithImage(DialCoreContainerImageTags.ChatImage, DialCoreContainerImageTags.ChatTag)
            .WithImageRegistry(DialCoreContainerImageTags.ChatRegistry)
            .WithHttpEndpoint(port: port, targetPort: 3000, name: "http")
            .WithUrlForEndpoint("http", r => r.DisplayText = "Chat")
            .WithEnvironment(context =>
                ConfigureDialChat(context, builder.Resource, themesBuilder.Resource)
            )
            .WaitFor(themesBuilder)
            .WaitFor(builder);

        configureContainer?.Invoke(chatBuilder);

        chatBuilder.WithParentRelationship(builder.Resource);
        themesBuilder.WithParentRelationship(builder.Resource);

        builder.Resource.ChatThemes = chatThemes;
        builder.Resource.Chat = chat;

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
