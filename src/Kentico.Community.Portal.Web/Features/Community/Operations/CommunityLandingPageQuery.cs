using CMS.ContentEngine;
using Kentico.Community.Portal.Core.Operations;
using Kentico.Content.Web.Mvc;

namespace Kentico.Community.Portal.Web.Features.Community;

public record CommunityLandingPageQuery(IRoutedWebPage Page) : WebPageRoutedQuery<CommunityLandingPageQueryResponse>(Page);
public record CommunityLandingPageQueryResponse(CommunityLandingPage Page);
public class CommunityLandingPageQueryHandler : WebPageQueryHandler<CommunityLandingPageQuery, CommunityLandingPageQueryResponse>
{
    public CommunityLandingPageQueryHandler(WebPageQueryTools tools) : base(tools) { }

    public override async Task<CommunityLandingPageQueryResponse> Handle(CommunityLandingPageQuery request, CancellationToken cancellationToken = default)
    {
        var b = new ContentItemQueryBuilder().ForWebPage(WebsiteChannelContext, CommunityLandingPage.CONTENT_TYPE_NAME, request.Page);

        var r = await Executor.GetWebPageResult(b, Mapper.Map<CommunityLandingPage>, DefaultQueryOptions, cancellationToken);

        return new(r.First());
    }
}
