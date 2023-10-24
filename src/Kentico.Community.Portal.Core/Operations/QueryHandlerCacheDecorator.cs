﻿using CMS.Helpers;
using Microsoft.Extensions.Options;

namespace Kentico.Community.Portal.Core.Operations;

/// <summary>
/// Integrates a caching layer into all <see cref="IQueryHandler{TQuery, TResult}"/>,
/// returning successful results from the cache and populating the <see cref="ICacheDependenciesStore"/>
/// with the related cache dependency keys
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class QueryHandlerCacheDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    private readonly IQueryHandler<TQuery, TResult> decorated;
    private readonly IProgressiveCache cache;

    /// <summary>
    /// Could be 0 or 1 items in this collection
    /// </summary>
    private readonly IEnumerable<IQueryCacheKeysCreator<TQuery, TResult>> creators;
    private readonly IEnumerable<IQueryCacheSettingsCustomizer<TQuery, TResult>> cacheCustomizers;
    private readonly ICacheDependenciesStore store;
    private readonly DefaultQueryCacheSettings settings;

    public QueryHandlerCacheDecorator(
        IQueryHandler<TQuery, TResult> decorated,
        IProgressiveCache cache,
        IEnumerable<IQueryCacheKeysCreator<TQuery, TResult>> creators,
        IEnumerable<IQueryCacheSettingsCustomizer<TQuery, TResult>> cacheCustomizers,
        ICacheDependenciesStore store,
        IOptions<DefaultQueryCacheSettings> settings)
    {
        this.decorated = decorated;
        this.cache = cache;
        this.creators = creators;
        this.cacheCustomizers = cacheCustomizers;
        this.store = store;
        this.settings = settings.Value;
    }

    public async Task<TResult> Handle(TQuery query, CancellationToken token = default)
    {
        var creator = creators.FirstOrDefault();

        if (!this.settings.IsEnabled || creator is null)
        {
            return await decorated.Handle(query, token);
        }

        var customizer = cacheCustomizers.FirstOrDefault();

        var settings = customizer?.CustomizeCacheSettings(this.settings, creator, query) ?? new CacheSettings(
            cacheMinutes: this.settings.CacheItemDuration.Minutes,
            useSlidingExpiration: true,
            cacheItemNameParts: creator.ItemNameParts(query));

        var entry = await cache.LoadAsync((cs, t) => GetCachedResult(query, store, creator, cs, t), settings, token);

        if (entry.Result is Result<TResult> monad && monad.IsSuccess)
        {
            store.Store(entry.Keys);
        }

        return entry.Result;
    }

    private async Task<CacheEntry<TResult>> GetCachedResult(
        TQuery query,
        ICacheDependenciesStore store,
        IQueryCacheKeysCreator<TQuery, TResult> cacheKeysCreator,
        CacheSettings cs, CancellationToken t)
    {
        var result = await decorated.Handle(query, t);

        if (result is Result<TResult> monad && monad.IsFailure)
        {
            cs.Cached = false;

            store.MarkCacheDisabled();

            return new CacheEntry<TResult>(result, Array.Empty<string>());
        }

        var resultValue = result is Result<TResult> success
            ? success.Value
            : result;

        string[] keys = cacheKeysCreator.DependencyKeys(query, resultValue);

        if (keys.Length == 0)
        {
            cs.Cached = false;

            store.MarkCacheDisabled();

            return new CacheEntry<TResult>(result, keys);
        }

        store.Store(keys);

        cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(keys);

        return new CacheEntry<TResult>(result, keys);
    }
}

/// <summary>
/// Represents a cache entry for a successful <see cref="Result{T}"/>
/// including the <paramref name="Keys"/> generated for the result
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Result"></param>
/// <param name="Keys"></param>
internal record struct CacheEntry<TResult>(TResult Result, string[] Keys);
