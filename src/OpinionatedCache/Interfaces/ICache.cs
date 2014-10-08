﻿// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System;
using System.Collections.Generic;

namespace OpinionatedCache.API
{
    public interface ICache
    {
        T Get<T>(string key) where T : class;
        IList<TElement> GetCollection<TCollectionKey, TElement>(TCollectionKey collectionKey)
            where TCollectionKey : IBaseCacheKey
            where TElement : class;

        void Clear();
        void Clear(IBaseCacheKey key);

        void Shutdown();
        void Snooze(int milliseconds);

        // internal implementation helpers
        Tuple<TElement, bool> EnrollSingle<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, TElement> filler)
            where TElement : class;

        Tuple<IList<TElement>, bool> EnrollCollection<TElement>(
                IBaseCacheKey key,
                Func<FreshnessRequest, IList<TElement>> filler,
                params Func<IBaseCacheKey, TElement, IBaseCacheKey>[] keyProjections)
            where TElement : class;
    }
}
