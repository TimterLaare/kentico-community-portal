﻿using Kentico.Community.Portal.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Kentico.Community.Portal.Web.Features.Errors;

[Route("error")]
public class HttpErrorsController(WebPageMetaService metaService) : Controller
{
    private readonly WebPageMetaService metaService = metaService;

    [HttpGet("{code:int}")]
    public ActionResult Error(int code)
    {
        if (code != 404)
        {
            metaService.SetMeta(new("Error", "There was a problem handling your request."));

            return StatusCode(code);
        }

        metaService.SetMeta(new("Not Found", "The page you requested could not be found."));

        return View("~/Features/Errors/NotFound.cshtml", new ErrorPageViewModel { StatusCode = code });
    }
}

public class ErrorPageViewModel
{
    public int StatusCode { get; set; }
}
