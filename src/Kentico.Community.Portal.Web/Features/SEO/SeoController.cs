﻿using System.Text;
using Kentico.Community.Portal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;

namespace Kentico.Community.Portal.Web.Features.SEO;

[Route("")]
public class SeoController(Sitemap sitemap, IMediator mediator) : Controller
{
    private readonly Sitemap sitemap = sitemap;
    private readonly IMediator mediator = mediator;

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var nodes = await sitemap.GetSitemapNodes();

        return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
    }

    [HttpGet("robots.txt")]
    public async Task<ActionResult> RobotsTxt()
    {
        var settings = await mediator.Send(new PortalWebsiteSettingsQuery());

        return Content(settings.GlobalContent.WebsiteGlobalContentRobotsTxt, "text/plain", Encoding.UTF8);
    }
}
