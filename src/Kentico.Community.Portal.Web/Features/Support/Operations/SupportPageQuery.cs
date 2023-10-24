using CMS.ContentEngine;
using Kentico.Community.Portal.Core.Operations;
using Kentico.Content.Web.Mvc;

namespace Kentico.Community.Portal.Web.Features.Support;

public record SupportPageQuery(IRoutedWebPage Page) : WebPageRoutedQuery<SupportPage>(Page);
public class SupportPageQueryHandler : WebPageQueryHandler<SupportPageQuery, SupportPage>
{
    public SupportPageQueryHandler(WebPageQueryTools tools) : base(tools) { }

    public override async Task<SupportPage> Handle(SupportPageQuery request, CancellationToken cancellationToken)
    {
        var b = new ContentItemQueryBuilder().ForWebPage(WebsiteChannelContext, SupportPage.CONTENT_TYPE_NAME, request.Page);

        var r = await Executor.GetWebPageResult(b, Mapper.Map<SupportPage>, DefaultQueryOptions, cancellationToken);

        return r.First();
    }
}
