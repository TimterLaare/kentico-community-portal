using CMS.ContentEngine;
using CMS.Core;
using Kentico.Community.Portal.Core;
using Kentico.Community.Portal.Core.Modules;
using Kentico.Community.Portal.Web.Features.QAndA;
using Kentico.Community.Portal.Web.Membership;
using Kentico.Content.Web.Mvc;
using MediatR;

namespace Kentico.Community.Portal.Web.Features.Blog.Events;

/// <summary>
/// Handles automatically creating a new Q and A Question
/// used for discussion of a blog post when a <see cref="BlogPostPage"/> is published
/// </summary>
public class BlogPostPublishCreateQAndAQuestionHandler(
    IHttpContextAccessor accessor,
    IMediator mediator,
    IContentQueryExecutor queryExecutor,
    IWebPageUrlRetriever pageUrlRetriever,
    ISystemClock clock,
    IEventLogService log)
{
    private readonly IHttpContextAccessor accessor = accessor;
    private readonly IMediator mediator = mediator;
    private readonly IContentQueryExecutor queryExecutor = queryExecutor;
    private readonly IWebPageUrlRetriever pageUrlRetriever = pageUrlRetriever;
    private readonly ISystemClock clock = clock;
    private readonly IEventLogService log = log;

    public async Task Handle(PublishWebPageEventArgs args)
    {
        /*
         * Only perform search indexing when a request is available (eg not during CI restore)
         */
        if (accessor.HttpContext is null)
        {
            return;
        }

        /*
         * We aren't using mediator for this query because it will return the cached page 
         * which will result in an infinite loop for the check below ... ask me how I know 😅😅
         */
        var b = new ContentItemQueryBuilder()
            .ForContentType(BlogPostPage.CONTENT_TYPE_NAME,
                q => q
                .Where(w => w.WhereEquals(nameof(WebPageFields.WebPageItemID), args.ID))
                .ForWebsite(args.WebsiteChannelName)
                .WithLinkedItems(BlogPostPage.FullQueryDepth));

        var pages = await queryExecutor.GetMappedWebPageResult<BlogPostPage>(b);

        if (pages.FirstOrDefault() is not BlogPostPage page)
        {
            return;
        }

        // Do not re-process pages that already have a linked Q&A Discussion Page
        if (!string.IsNullOrEmpty(page.BlogPostPageQAndADiscussionLinkPath) || page.BlogPostPageQAndADiscussionPage.Any())
        {
            return;
        }

        var rootQuestionPage = await mediator.Send(new QAndAQuestionsRootPageQuery(args.WebsiteChannelName));
        var now = clock.Now;
        var questionMonthFolder = await mediator.Send(new QAndAMonthFolderQuery(rootQuestionPage, args.WebsiteChannelName, now.Year, now.Month, args.WebsiteChannelID));

        var url = await pageUrlRetriever.Retrieve(page);
        string postTitle = page.BlogPostPageBlogPostContent
            .TryFirst()
            .Map(b => b.ListableItemTitle)
            .GetValueOrDefault("");
        string questionTitle = $"Blog Discussion: {postTitle}";
        string questionContent = $"""
            Blog Post: [{postTitle}]({url.RelativePathTrimmed()})

            Continue discussions 🤗 on this blog post below.
            """;
        var member = new CommunityMember()
        {
            Id = 0 // Only the Id is required and an Id of 0 will result in the author being the Kentico Community author
        };
        var command = new QAndAQuestionCreateCommand(
            member,
            questionMonthFolder,
            args.WebsiteChannelID,
            questionTitle,
            questionContent,
            SystemTaxonomies.QAndADiscussionTypeTaxonomy.BlogTag.GUID,
            page);

        await mediator.Send(command, default)
            .Map(async questionWebPageID =>
            {
                /*
                * This will recurse on this handler because it publishes the blog post
                * but we guard against updating a blog post page that already has a question path
                */
                return await mediator.Send(new BlogPostPageSetQuestionURLCommand(page, args.WebsiteChannelID, questionWebPageID));
            })
            .Match(
                MonadUtilities.Noop,
                err => log.LogError(nameof(BlogPostPublishCreateQAndAQuestionHandler), "UPDATE_BLOG_QUESTION_URL", err));
    }
}
