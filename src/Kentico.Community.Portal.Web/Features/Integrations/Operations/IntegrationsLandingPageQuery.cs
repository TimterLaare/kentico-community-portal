using CMS.ContentEngine;
using Kentico.Community.Portal.Core.Operations;
using Kentico.Content.Web.Mvc;

namespace Kentico.Community.Portal.Web.Features.Integrations;

public record IntegrationsLandingPageQuery(IRoutedWebPage Page) : WebPageRoutedQuery<IntegrationsLandingPageQueryResponse>(Page);
public record IntegrationsLandingPageQueryResponse(IntegrationsLandingPage Page);
public class IntegrationsLandingPageQueryHandler : WebPageQueryHandler<IntegrationsLandingPageQuery, IntegrationsLandingPageQueryResponse>
{
    public IntegrationsLandingPageQueryHandler(WebPageQueryTools tools) : base(tools) { }

    public override async Task<IntegrationsLandingPageQueryResponse> Handle(IntegrationsLandingPageQuery request, CancellationToken cancellationToken = default)
    {
        var b = new ContentItemQueryBuilder().ForWebPage(WebsiteChannelContext, IntegrationsLandingPage.CONTENT_TYPE_NAME, request.Page);

        var r = await Executor.GetWebPageResult(b, Mapper.Map<IntegrationsLandingPage>, DefaultQueryOptions, cancellationToken);

        return new(r.First());
    }
}
