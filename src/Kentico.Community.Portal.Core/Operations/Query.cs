using Kentico.Content.Web.Mvc;
using MediatR;

namespace Kentico.Community.Portal.Core.Operations;

public interface IQuery<out TResult> : IRequest<TResult> { }
public abstract record WebPageRoutedQuery<TResult>(IRoutedWebPage Page) : IQuery<TResult>, ICacheByValueQuery
{
    public virtual string CacheValueKey => $"{Page.WebPageItemID}|{Page.LanguageName}";
}

public abstract record WebPageByGUIDQuery<TResult>(Guid WebPageGUID) : IQuery<TResult>, ICacheByValueQuery
{
    public virtual string CacheValueKey => WebPageGUID.ToString();
}

public abstract record WebPageByIDQuery<TResult>(int WebPageID) : IQuery<TResult>, ICacheByValueQuery
{
    public string CacheValueKey => WebPageID.ToString();
}

public abstract record ContentItemQuery<TResult>() : IQuery<TResult>;
public interface ICacheByValueQuery
{
    string CacheValueKey { get; }
}
