using System.Reflection;
using Kentico.Community.Portal.Core;
using Kentico.Community.Portal.Core.Infrastructure;
using Kentico.Community.Portal.Core.Modules;
using Kentico.Community.Portal.Core.Operations;
using Kentico.Community.Portal.Web.Components.ViewComponents.Navigation;
using Kentico.Community.Portal.Web.Components.Widgets.Licenses;
using Kentico.Community.Portal.Web.Features.Blog.Events;
using Kentico.Community.Portal.Web.Features.DataCollection;
using Kentico.Community.Portal.Web.Features.Forms;
using Kentico.Community.Portal.Web.Features.Home;
using Kentico.Community.Portal.Web.Features.Members.Badges;
using Kentico.Community.Portal.Web.Features.QAndA.Events;
using Kentico.Community.Portal.Web.Features.SEO;
using Kentico.Community.Portal.Web.Features.Support;
using Kentico.Community.Portal.Web.Infrastructure;
using Kentico.Community.Portal.Web.Infrastructure.Storage;
using Kentico.Community.Portal.Web.Rendering;
using Kentico.Community.Portal.Web.Rendering.Events;
using Sidio.Sitemap.AspNetCore;
using Sidio.Sitemap.Core.Services;
using Slugify;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionAppExtensions
{
    public static IServiceCollection AddApp(this IServiceCollection services, IConfiguration config) =>
        services
            .AddScoped<LicensesFacade>()
            .AddScoped<CookieConsentManager>()
            .AddScoped<ConsentManager>()
            .AddRendering()
            .AddSEO()
            .AddForms(config)
            .AddOperations(config)
            .AddInfrastructure(config)
            .AddSupport(config)
            .AddQAndA()
            .AddBlogs()
            .AddMemberBadges();

    private static IServiceCollection AddOperations(this IServiceCollection services, IConfiguration config) =>
        services
            .AddSingleton<ICacheDependencyKeysBuilder, CacheDependencyKeysBuilder>()
            .Configure<DefaultQueryCacheSettings>(settings =>
            {
                var section = config.GetSection("Cache:Query");

                settings.CacheItemDuration = TimeSpan.FromMinutes(section.GetValue<int>("CacheItemDuration"));
                settings.IsEnabled = section.GetValue<bool>("IsEnabled");
                settings.IsSlidingExpiration = section.GetValue<bool>("IsSlidingExpiration");
            })
            .AddMediatR(c => c
                .RegisterServicesFromAssembly(typeof(HomePageQuery).Assembly)
                .AddOpenBehavior(typeof(QueryCachingPipelineBehavior<,>))
                .AddOpenBehavior(typeof(CommandHandlerLogDecorator<,>)))
            .AddClosedGenericTypes(typeof(HomePageQuery).Assembly, typeof(IQueryHandler<,>), ServiceLifetime.Transient)
            .AddClosedGenericTypes(typeof(HomePageQuery).Assembly, typeof(ICommandHandler<,>), ServiceLifetime.Transient)
            .AddTransient<WebPageCommandTools>()
            .AddTransient<IContentQueryExecutionOptionsCreator, DefaultContentQueryExecutionOptionsCreator>()
            .AddTransient<WebPageQueryTools>()
            .AddTransient<ContentItemQueryTools>()
            .AddTransient<DataItemCommandTools>()
            .AddTransient<DataItemQueryTools>()
            .AddSingleton<IChannelDataProvider, ChannelDataProvider>()
            .AddTransient<AlertMessageCookieManager>()
            .AddSingleton(_ => TimeProvider.System);

    private static IServiceCollection AddRendering(this IServiceCollection services) =>
        services
            .AddSingleton(s => new MarkdownRenderer())
            .AddSingleton<ISlugHelper>(_ => new SlugHelper(new SlugHelperConfiguration()))
            .AddScoped<ViewService>()
            .AddScoped<AvatarImageService>()
            .AddScoped<ClientAssets>()
            .AddScoped<IJSEncoder, JSEncoder>();

    private static IServiceCollection AddSEO(this IServiceCollection services) =>
        services
            .AddScoped<WebPageMetaService>()
            .AddTransient<SitemapRetriever>()
            .AddDefaultSitemapServices<HttpContextBaseUrlProvider>();

    private static IServiceCollection AddForms(this IServiceCollection services, IConfiguration config) =>
        services
            .AddSingleton<FormInternalAutoresponderEmailSender>()
            .Configure<SystemDomainOptions>(config.GetSection("SystemDomains"));

    private static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) =>
        services
            .AddSingleton(s => new ApplicationAssemblyInformation())
            .AddSingleton<AzureStorageClientFactory>()
            .AddSingleton<ISystemClock, SystemClock>()
            .AddSingleton<AssetItemService>()
            .AddSingleton<IStoragePathService, StoragePathService>()
            .AddTransient<MediaAssetContentMetadataHandler>()
            .AddScoped<CaptchaValidator>()
            .Configure<ReCaptchaSettings>(config.GetSection("ReCaptcha"));


    private static IServiceCollection AddSupport(this IServiceCollection services, IConfiguration config) =>
        services
            .AddHostedService<SupportRequestProcessorBackgroundService>()
            .AddSingleton<ISupportEmailSender, SupportEmailSender>()
            .Configure<SupportRequestProcessingSettings>(config.GetSection("SupportRequestProcessing"));

    private static IServiceCollection AddQAndA(this IServiceCollection services) =>
        services
            .AddTransient<QAndAAnswerCreateSearchIndexTaskHandler>()
            .AddTransient<QAndAAnswerDeleteSearchIndexTaskHandler>();
    private static IServiceCollection AddBlogs(this IServiceCollection services) =>
        services.AddTransient<BlogPostPublishCreateQAndAQuestionHandler>();

    private static IServiceCollection AddMemberBadges(this IServiceCollection services) =>
        services
            .AddSingleton<IMemberBadgeInfoProvider, MemberBadgeInfoProvider>()
            .AddSingleton<IMemberBadgeMemberInfoProvider, MemberBadgeMemberInfoProvider>()
            .AddTransient<MemberBadgeService>()
            .AddTransient<IMemberBadgeAssignmentRule, BlogPostAuthorMemberBadgeAssignmentRule>()
            // This rule is disabled because it has already run and no new members can qualify for it. It is left here as an example.
            //.AddTransient<IMemberBadgeAssignmentRule, EarlyAdopterMemberBadgeAssignmentRule>()
            .AddTransient<IMemberBadgeAssignmentRule, DiscussionAuthorMemberBadgeAssignmentRule>()
            .AddTransient<IMemberBadgeAssignmentRule, DiscussionParticipantMemberBadgeAssignmentRule>()
            .AddTransient<IMemberBadgeAssignmentRule, DiscussionAcceptedAnswerAuthorMemberBadgeAssignmentRule>()
            .AddTransient<IMemberBadgeAssignmentRule, KenticoEmployeeMemberBadgeAssignmentRule>()
            .AddTransient<IMemberBadgeAssignmentRule, IntegrationAuthorMemberBadgeAssignmentRule>()
            .AddHostedService<MemberBadgeAssignmentApplicationBackgroundService>();

    private static IServiceCollection AddClosedGenericTypes(
        this IServiceCollection services,
        Assembly assembly,
        Type typeToRegister,
        ServiceLifetime serviceLifetime)
    {
        _ = services.Scan(x => x.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeToRegister)
                .Where(t => !t.IsGenericType))
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }
}
